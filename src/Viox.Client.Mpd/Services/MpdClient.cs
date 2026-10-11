namespace Viox.Client.Mpd.Services;

using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Client.Mpd.Configuration;
using Viox.Core.Models;
using Viox.Core.Services;

/// <summary>
/// Provides data for MPD subsystem change events.
/// </summary>
public sealed class MpdEventArgs : EventArgs
{
    /// <summary>
    /// Gets the name of the MPD subsystem that changed (e.g., player, playlist, mixer).
    /// </summary>
    public string Subsystem { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MpdEventArgs"/> class.
    /// </summary>
    /// <param name="subsystem">The changed subsystem name.</param>
    public MpdEventArgs(string subsystem)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subsystem);
        Subsystem = subsystem;
    }
}

/// <summary>
/// A managed TCP client implementation for interacting with a Music Player Daemon (MPD) server.
/// </summary>
public sealed class MpdClient : IMpdClient, IHostedService, IAsyncDisposable, IDisposable
{
    private readonly MpdOptions _options;
    private readonly ILogger<MpdClient> _logger;
    private readonly IMediaEventService _mediaEventService;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private StreamReader? _reader;
    private StreamWriter? _writer;

    // Background idle client fields
    private TcpClient? _idleTcpClient;
    private NetworkStream? _idleStream;
    private StreamReader? _idleReader;
    private StreamWriter? _idleWriter;
    private CancellationTokenSource? _backgroundCts;
    private Task? _idleTask;
    private readonly SemaphoreSlim _idleLock = new(1, 1);

    private bool _disposed;
    private string? _lastKnownState;

    /// <summary>
    /// Occurs when an MPD subsystem event is raised by the server.
    /// </summary>
    public event EventHandler<MpdEventArgs>? SubsystemChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="MpdClient"/> class.
    /// </summary>
    /// <param name="options">The strongly typed configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="mediaEventService">The media event service for publishing actions.</param>
    public MpdClient(
        IOptions<MpdOptions> options,
        ILogger<MpdClient> logger,
        IMediaEventService mediaEventService)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mediaEventService);

        _options = options.Value;
        _logger = logger;
        _mediaEventService = mediaEventService;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting MPD background client service...");
        _backgroundCts = new CancellationTokenSource();

        // Fire-and-forget background connection loop so it doesn't block application startup
        _ = RunBackgroundLifecycleAsync(_backgroundCts.Token);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopClientAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping MPD background client service...");
        _backgroundCts?.Cancel();
        await DisconnectAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task RunBackgroundLifecycleAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && !_disposed)
        {
            try
            {
                await ConnectAsync(cancellationToken).ConfigureAwait(false);
                break; // Connected successfully, exit retry loop
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to connect to MPD server in background. Retrying in 5 seconds...");
                try
                {
                    await Task.Delay(5000, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    /// <inheritdoc />
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_tcpClient is { Connected: true } && _stream is not null)
        {
            _logger.LogInformation("Connection already established to {Host}:{Port}", _options.Host, _options.Port);
            return;
        }

        await _connectionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_tcpClient is { Connected: true } && _stream is not null)
            {
                return;
            }

            CleanupCore();

            _logger.LogInformation("Connecting to MPD server at {Host}:{Port}...", _options.Host, _options.Port);

            _tcpClient = new TcpClient();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_options.TimeoutMilliseconds);

            await _tcpClient.ConnectAsync(_options.Host, _options.Port, cts.Token).ConfigureAwait(false);

            _stream = _tcpClient.GetStream();
            _reader = new StreamReader(_stream, Encoding.UTF8, leaveOpen: true);
            _writer = new StreamWriter(_stream, new UTF8Encoding(false), leaveOpen: true)
            {
                AutoFlush = true
            };

            string? greeting = await _reader.ReadLineAsync(cts.Token).ConfigureAwait(false);
            if (greeting is null || !greeting.StartsWith("OK MPD", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Invalid greeting received from MPD server: {greeting}");
            }

            _logger.LogInformation("Connected successfully. Server banner: {Banner}", greeting);

            if (!string.IsNullOrEmpty(_options.Password))
            {
                _logger.LogDebug("Sending password authentication...");
                await SendCommandInternalAsync($"password {_options.Password}", cancellationToken).ConfigureAwait(false);
            }

            await RefreshPlayerStateAsync(cancellationToken).ConfigureAwait(false);
            StartIdleLoop();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish connection to MPD server at {Host}:{Port}", _options.Host, _options.Port);
            CleanupCore();
            throw;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task RefreshPlayerStateAsync(CancellationToken cancellationToken)
    {
        try
        {
            string statusResponse = await SendCommandInternalAsync("status", cancellationToken).ConfigureAwait(false);
            _lastKnownState = ExtractStatusValue(statusResponse, "state");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch initial player state.");
        }
    }

    private static string? ExtractStatusValue(string statusResponse, string keyName)
    {
        using var reader = new StringReader(statusResponse);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            int colonIndex = line.IndexOf(':');
            if (colonIndex <= 0) continue;

            ReadOnlySpan<char> key = line.AsSpan(0, colonIndex).Trim();
            ReadOnlySpan<char> value = line.AsSpan(colonIndex + 1).Trim();

            if (key.Equals(keyName, StringComparison.OrdinalIgnoreCase))
            {
                return value.ToString();
            }
        }
        return null;
    }

    private void StartIdleLoop()
    {
        _idleTask = RunIdleLoopAsync(_backgroundCts?.Token ?? CancellationToken.None);
    }

    private async Task RunIdleLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && !_disposed)
        {
            try
            {
                await _idleLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    CleanupIdleCore();

                    _idleTcpClient = new TcpClient();
                    await _idleTcpClient.ConnectAsync(_options.Host, _options.Port, cancellationToken).ConfigureAwait(false);

                    _idleStream = _idleTcpClient.GetStream();
                    _idleReader = new StreamReader(_idleStream, Encoding.UTF8, leaveOpen: true);
                    _idleWriter = new StreamWriter(_idleStream, new UTF8Encoding(false), leaveOpen: true)
                    {
                        AutoFlush = true
                    };

                    string? greeting = await _idleReader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                    if (greeting is null || !greeting.StartsWith("OK MPD", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException($"Invalid greeting received on idle connection: {greeting}");
                    }

                    if (!string.IsNullOrEmpty(_options.Password))
                    {
                        await _idleWriter.WriteLineAsync($"password {_options.Password}".AsMemory(), cancellationToken).ConfigureAwait(false);
                        string? authResponse = await _idleReader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                        if (authResponse is null || !authResponse.StartsWith("OK", StringComparison.Ordinal))
                        {
                            throw new InvalidOperationException($"Idle connection authentication failed: {authResponse}");
                        }
                    }
                }
                finally
                {
                    _idleLock.Release();
                }

                while (!cancellationToken.IsCancellationRequested && !_disposed)
                {
                    await _idleLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                    try
                    {
                        if (_idleWriter is null || _idleReader is null)
                        {
                            break;
                        }

                        await _idleWriter.WriteLineAsync("idle".AsMemory(), cancellationToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        _idleLock.Release();
                    }

                    while (!cancellationToken.IsCancellationRequested && !_disposed)
                    {
                        string? line;
                        await _idleLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                        try
                        {
                            if (_idleReader is null)
                            {
                                break;
                            }
                            line = await _idleReader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                        }
                        finally
                        {
                            _idleLock.Release();
                        }

                        if (line is null)
                        {
                            throw new IOException("Idle connection closed prematurely by server.");
                        }

                        if (line.Equals("OK", StringComparison.Ordinal))
                        {
                            break;
                        }

                        if (line.StartsWith("ACK", StringComparison.Ordinal))
                        {
                            _logger.LogWarning("MPD Idle command returned error: {Error}", line);
                            break;
                        }

                        int colonIndex = line.IndexOf(':');
                        if (colonIndex > 0)
                        {
                            ReadOnlySpan<char> key = line.AsSpan(0, colonIndex).Trim();
                            ReadOnlySpan<char> value = line.AsSpan(colonIndex + 1).Trim();
                            if (key.Equals("changed", StringComparison.OrdinalIgnoreCase))
                            {
                                string subsystem = value.ToString();
                                _logger.LogDebug("MPD subsystem changed: {Subsystem}", subsystem);
                                await OnSubsystemChangedAsync(subsystem, cancellationToken).ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error in background MPD idle loop. Reconnecting idle client in 3 seconds...");
                try
                {
                    await Task.Delay(3000, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    private async Task OnSubsystemChangedAsync(string subsystem, CancellationToken cancellationToken)
    {
        SubsystemChanged?.Invoke(this, new MpdEventArgs(subsystem));

        try
        {
            if (subsystem.Equals("player", StringComparison.OrdinalIgnoreCase))
            {
                string statusResponse = await SendCommandInternalAsync("status", cancellationToken).ConfigureAwait(false);
                string? currentState = ExtractStatusValue(statusResponse, "state");

                bool transitionedToStop =
                    _lastKnownState is "play" or "pause" &&
                    currentState is "stop";

                _lastKnownState = currentState;

                if (transitionedToStop)
                {
                    _logger.LogInformation("Track finished playing. Firing player-stopped action.");
                    await _mediaEventService.PublishMediaActionAsync("player-stopped", null, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    string currentSongResponse = await SendCommandInternalAsync("currentsong", cancellationToken).ConfigureAwait(false);
                    MediaMetaData? metadata = ParseMetadata(currentSongResponse);

                    await _mediaEventService.PublishMediaActionAsync("player-changed", metadata, cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                await _mediaEventService.PublishMediaActionAsync(subsystem, null, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish media action for subsystem {Subsystem}", subsystem);
        }
    }

    private static MediaMetaData? ParseMetadata(string currentSongResponse)
    {
        using var reader = new StringReader(currentSongResponse);
        string? line;
        string? title = null;
        string? artist = null;
        string? album = null;
        string? file = null;

        while ((line = reader.ReadLine()) is not null)
        {
            int colonIndex = line.IndexOf(':');
            if (colonIndex <= 0) continue;

            ReadOnlySpan<char> key = line.AsSpan(0, colonIndex).Trim();
            ReadOnlySpan<char> value = line.AsSpan(colonIndex + 1).Trim();

            if (key.Equals("Title", StringComparison.OrdinalIgnoreCase))
            {
                title = value.ToString();
            }
            else if (key.Equals("Artist", StringComparison.OrdinalIgnoreCase))
            {
                artist = value.ToString();
            }
            else if (key.Equals("Album", StringComparison.OrdinalIgnoreCase))
            {
                album = value.ToString();
            }
            else if (key.Equals("file", StringComparison.OrdinalIgnoreCase))
            {
                file = value.ToString();
            }
        }

        if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(artist) && string.IsNullOrEmpty(file))
        {
            return null;
        }

        return new MediaMetaData
        {
            Title = title ?? string.Empty,
            Artist = artist ?? string.Empty,
            Album = album ?? string.Empty,
            Url = file ?? string.Empty,
            ImageUrl = string.Empty
        };
    }

    /// <inheritdoc />
    public async Task<string> SendCommandAsync(string command, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(command);

        const int maxAttempts = 2;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await ConnectAsync(cancellationToken).ConfigureAwait(false);
                return await SendCommandInternalAsync(command, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is SocketException or IOException)
            {
                _logger.LogWarning(ex, "Network error occurred while executing command '{Command}' (Attempt {Attempt}/{Max}). Reconnecting...", command, attempt, maxAttempts);

                await _connectionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    CleanupCore();
                }
                finally
                {
                    _connectionLock.Release();
                }

                if (attempt == maxAttempts)
                {
                    throw;
                }
            }
        }

        return string.Empty;
    }

    private async Task<string> SendCommandInternalAsync(string command, CancellationToken cancellationToken)
    {
        await _connectionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_writer is null || _reader is null)
            {
                throw new InvalidOperationException("Client streams are uninitialized.");
            }

            _logger.LogDebug("Sending command: {Command}", command);
            await _writer.WriteLineAsync(command.AsMemory(), cancellationToken).ConfigureAwait(false);

            var builder = new StringBuilder();
            while (true)
            {
                string? line = await _reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                if (line is null)
                {
                    throw new IOException("Connection closed prematurely by the MPD server.");
                }

                if (line.Equals("OK", StringComparison.Ordinal))
                {
                    break;
                }

                if (line.StartsWith("ACK", StringComparison.Ordinal))
                {
                    _logger.LogError("MPD Command returned error: {Error}", line);
                    throw new InvalidOperationException($"MPD Error response: {line}");
                }

                builder.AppendLine(line);
            }

            return builder.ToString();
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <inheritdoc />
    public Task PlayAsync(CancellationToken cancellationToken = default) => SendCommandAsync("play", cancellationToken);

    /// <inheritdoc />
    public async Task PlayFileOrUrlAsync(string fileOrUrl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileOrUrl);

        _logger.LogInformation("Loading and playing path or URL: {Path}", fileOrUrl);
        await ClearPlaylistAsync(cancellationToken).ConfigureAwait(false);

        string escapedPath = fileOrUrl.Replace("\"", "\\\"", StringComparison.Ordinal);

        if (!fileOrUrl.StartsWith("http"))
        {
            escapedPath = fileOrUrl.Replace("/music/", "").Replace("\"", "\\\"", StringComparison.Ordinal);
        }

        _logger.LogInformation("Add '{escapedPath}' to MPD", escapedPath);

        await SendCommandAsync($"add \"{escapedPath}\"", cancellationToken).ConfigureAwait(false);
        await PlayAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task ClearPlaylistAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Clearing active MPD playlist.");
        return SendCommandAsync("clear", cancellationToken);
    }

    /// <inheritdoc />
    public Task PauseAsync(CancellationToken cancellationToken = default) => SendCommandAsync("pause 1", cancellationToken);

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken = default) => SendCommandAsync("stop", cancellationToken);

    /// <inheritdoc />
    public Task NextAsync(CancellationToken cancellationToken = default) => SendCommandAsync("next", cancellationToken);

    /// <inheritdoc />
    public Task PreviousAsync(CancellationToken cancellationToken = default) => SendCommandAsync("previous", cancellationToken);

    /// <inheritdoc />
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Closing connection to MPD server...");

            _backgroundCts?.Cancel();
            if (_idleTask is not null)
            {
                try
                {
                    await _idleTask.ConfigureAwait(false);
                }
                catch
                {
                    // Suppress during disconnect
                }
            }

            if (_tcpClient is { Connected: true })
            {
                await SendCommandInternalAsync("close", cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Exception encountered while closing connection gracefully.");
        }
        finally
        {
            await _connectionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                CleanupCore();
            }
            finally
            {
                _connectionLock.Release();
            }

            await _idleLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                CleanupIdleCore();
            }
            finally
            {
                _idleLock.Release();
            }
        }
    }

    private void CleanupCore()
    {
        _writer?.Dispose();
        _reader?.Dispose();
        _stream?.Dispose();
        _tcpClient?.Dispose();

        _writer = null;
        _reader = null;
        _stream = null;
        _tcpClient = null;
    }

    private void CleanupIdleCore()
    {
        _idleWriter?.Dispose();
        _idleReader?.Dispose();
        _idleStream?.Dispose();
        _idleTcpClient?.Dispose();

        _idleWriter = null;
        _idleReader = null;
        _idleStream = null;
        _idleTcpClient = null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _backgroundCts?.Cancel();
        CleanupCore();
        CleanupIdleCore();
        _connectionLock.Dispose();
        _idleLock.Dispose();
        _backgroundCts?.Dispose();
        _disposed = true;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await DisconnectAsync().ConfigureAwait(false);
        CleanupCore();
        CleanupIdleCore();
        _connectionLock.Dispose();
        _idleLock.Dispose();
        _backgroundCts?.Dispose();
        _disposed = true;
    }

    /// <summary>
    /// Gets the current playback position of the active track in seconds.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The current playback position in seconds, or <see langword="null"/> if stopped or unavailable.</returns>
    public async Task<double?> GetCurrentPositionAsync(CancellationToken cancellationToken = default)
    {
        string response = await SendCommandAsync("status", cancellationToken).ConfigureAwait(false);
        using var reader = new StringReader(response);
        string? line;
        double? elapsedSeconds = null;
        while ((line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false)) is not null)
        {
            int colonIndex = line.IndexOf(':');
            if (colonIndex <= 0)
            {
                continue;
            }
            ReadOnlySpan<char> key = line.AsSpan(0, colonIndex).Trim();
            ReadOnlySpan<char> value = line.AsSpan(colonIndex + 1).Trim();
            if (key.Equals("elapsed", StringComparison.OrdinalIgnoreCase))
            {
                if (double.TryParse(value, NumberStyles.Float | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double parsedElapsed))
                {
                    return parsedElapsed;
                }
            }
            else if (key.Equals("time", StringComparison.OrdinalIgnoreCase) && elapsedSeconds is null)
            {
                int separatorIndex = value.IndexOf(':');
                ReadOnlySpan<char> elapsedPart = separatorIndex >= 0 ? value[..separatorIndex] : value;
                if (double.TryParse(elapsedPart, NumberStyles.Float | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double parsedTime))
                {
                    elapsedSeconds = parsedTime;
                }
            }
        }
        return elapsedSeconds;
    }
}
