namespace Viox.Server.Adapters;

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Viox.Client.Mpd.Services;
using Viox.Server.Abstraction;

/// <summary>
/// Adapter wrapping IMpdClient for video, local audio, and general network media streams.
/// </summary>
public sealed class MpdMediaPlayerAdapter : IMediaPlayerAdapter
{
    private readonly IMpdClient _client;
    private readonly ILogger<MpdMediaPlayerAdapter> _logger;

    public bool UseProxy => true;

    /// <summary>
    /// Gets the name of the media player adapter.
    /// </summary>
    public string Name => "MPD";

    /// <summary>
    /// Initializes a new instance of the <see cref="MpdMediaPlayerAdapter"/> class.
    /// </summary>
    /// <param name="client">The MPD client instance.</param>
    /// <param name="logger">The logger instance.</param>
    public MpdMediaPlayerAdapter(
        IMpdClient client,
        ILogger<MpdMediaPlayerAdapter> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Determines whether this adapter can handle the given URI.
    /// </summary>
    /// <param name="uri">The target media URI.</param>
    /// <returns><c>true</c> if the URI can be handled by MPD; otherwise, <c>false</c>.</returns>
    public bool CanHandle(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
        {
            return false;
        }

        bool isSpotify = uri.StartsWith("spotify:", StringComparison.OrdinalIgnoreCase) ||
                        uri.Contains("open.spotify.com", StringComparison.OrdinalIgnoreCase);

        return !isSpotify;
    }

    /// <summary>
    /// Checks whether the MPD player is currently playing active media.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns><c>true</c> if state is playing; otherwise, <c>false</c>.</returns>
    public async Task<bool> IsActiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            string status = await _client.SendCommandAsync("status", cancellationToken);
            return status.Contains("state: play", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to query MPD active status.");
            return false;
        }
    }

    /// <summary>
    /// Loads and plays the specified URI immediately.
    /// </summary>
    /// <param name="uri">The file path or stream URL.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    public async Task PlayAsync(string uri, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Routing playback to MPD for URI: {Uri}", uri);
        await _client.PlayFileOrUrlAsync(uri, cancellationToken);
    }

    /// <summary>
    /// Pauses active playback.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    public async Task PauseAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Pausing MPD playback.");
        await _client.PauseAsync(cancellationToken);
    }

    /// <summary>
    /// Resumes active playback.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    public async Task ResumeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Resuming MPD playback.");
        await _client.PlayAsync(cancellationToken);
    }

    /// <summary>
    /// Stops active playback.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stopping MPD playback.");
        await _client.StopAsync(cancellationToken);
    }

    /// <summary>
    /// Seeks playback to the specified position in time.
    /// </summary>
    /// <param name="position">The target position offset.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    public async Task SeekAsync(TimeSpan position, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeking MPD playback to {Seconds} seconds.", position.TotalSeconds);
        string totalSeconds = Math.Max(0, (int)position.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        await _client.SendCommandAsync($"seekcur {totalSeconds}", cancellationToken);
    }

    /// <summary>
    /// Navigates to the next item in the active playlist.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    public async Task NextAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Navigating to next item in MPD playlist.");
        await _client.NextAsync(cancellationToken);
    }

    /// <summary>
    /// Navigates to the previous item in the active playlist.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    public async Task PreviousAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Navigating to previous item in MPD playlist.");
        await _client.PreviousAsync(cancellationToken);
    }

    /// <summary>
    /// Sets the playback volume level between 0 and 100.
    /// </summary>
    /// <param name="volumePercent">The desired volume percentage.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    public async Task SetVolumeAsync(int volumePercent, CancellationToken cancellationToken = default)
    {
        int clamped = Math.Clamp(volumePercent, 0, 100);
        _logger.LogInformation("Setting MPD volume to {Volume}%.", clamped);
        await _client.SendCommandAsync($"setvol {clamped}", cancellationToken);
    }

    /// <summary>
    /// Retrieves the current volume level from the MPD server.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>The volume level percentage (0 to 100).</returns>
    public async Task<int> GetVolumeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            string statusResponse = await _client.SendCommandAsync("status", cancellationToken);
            using var reader = new System.IO.StringReader(statusResponse);

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("volume:", StringComparison.OrdinalIgnoreCase))
                {
                    string volumePart = line["volume:".Length..].Trim();
                    if (int.TryParse(volumePart, NumberStyles.Integer, CultureInfo.InvariantCulture, out int volume))
                    {
                        return Math.Clamp(volume, 0, 100);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve MPD volume status.");
        }

        return 0;
    }
}
