namespace Viox.Client.Mpd.Services;

using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Client.Mpd.Configuration;

/// <summary>
/// A managed TCP client implementation for interacting with a Music Player Daemon (MPD) server.
/// </summary>
public sealed class MpdClient : IMpdClient, IAsyncDisposable, IDisposable
{
    private readonly MpdOptions _options;
    private readonly ILogger<MpdClient> _logger;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MpdClient"/> class.
    /// </summary>
    /// <param name="options">The strongly typed configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public MpdClient(IOptions<MpdOptions> options, ILogger<MpdClient> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _options = options.Value;
        _logger = logger;
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
        if (_tcpClient is not { Connected: true })
        {
            return;
        }

        try
        {
            _logger.LogInformation("Closing connection to MPD server...");
            await SendCommandInternalAsync("close", cancellationToken).ConfigureAwait(false);
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

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        CleanupCore();
        _connectionLock.Dispose();
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
        _connectionLock.Dispose();
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
                // Fallback for older MPD protocol responses formatted as "elapsed:total"
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
