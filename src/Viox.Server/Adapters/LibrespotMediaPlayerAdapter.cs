namespace Viox.Server.Adapters;

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Viox.Client.Librespot.Models;
using Viox.Client.Librespot.Services;
using Viox.Server.Abstraction;

/// <summary>
/// Adapter wrapping ILibrespotRestClient to support Spotify playback.
/// </summary>
public sealed class LibrespotMediaPlayerAdapter : IMediaPlayerAdapter
{
    private readonly ILibrespotRestClient _client;
    private readonly ILogger<LibrespotMediaPlayerAdapter> _logger;

    public bool UseProxy => false;
    public string Name => "Librespot";

    public LibrespotMediaPlayerAdapter(
        ILibrespotRestClient client,
        ILogger<LibrespotMediaPlayerAdapter> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool CanHandle(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
        {
            return false;
        }

        return uri.StartsWith("spotify:", StringComparison.OrdinalIgnoreCase) ||
               uri.Contains("open.spotify.com", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<bool> IsActiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            ApiStatus? status = await _client.GetStatusAsync(cancellationToken);
            return status != null && status.IsPlaying;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to query Librespot playback status.");
            return false;
        }
    }

    public async Task<double> GetPlaybackPositionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            ApiStatus? status = await _client.GetStatusAsync(cancellationToken);
            if (status is null || status.Track is null)
            {
                return 0;
            }

            return (status.Track.Position / 1000);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to query Librespot playback position.");
            return 0;
        }
    }

    public async Task PlayAsync(string uri, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Routing playback to Librespot for Spotify URI: {Uri}", uri);
        var request = new ApiPlayRequest { Uri = uri };
        await _client.PlayAsync(request, cancellationToken);
    }

    public async Task PauseAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Pausing Librespot playback.");
        await _client.PauseAsync(cancellationToken);
    }

    public async Task ResumeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Resuming Librespot playback.");
        await _client.ResumeAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stopping Librespot playback.");
        await _client.StopAsync(cancellationToken);
    }

    public async Task SeekAsync(TimeSpan position, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeking Librespot playback to {Position}.", position);
        var request = new ApiSeekRequest { Position = (long)position.TotalMilliseconds, Relative = false };
        await _client.SeekAsync(request, cancellationToken);
    }

    public async Task NextAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Advancing to next track in Librespot.");
        await _client.NextAsync(null, cancellationToken);
    }

    public async Task PreviousAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Going to previous track in Librespot.");
        await _client.PreviousAsync(cancellationToken);
    }

    public async Task SetVolumeAsync(int volumePercent, CancellationToken cancellationToken = default)
    {
        int clamped = Math.Clamp(volumePercent, 0, 100);
        _logger.LogInformation("Setting Librespot volume to {Volume}%.", clamped);
        var request = new ApiSetVolumeRequest { Volume = clamped };
        await _client.SetVolumeAsync(request, cancellationToken);
    }

    public async Task<int> GetVolumeAsync(CancellationToken cancellationToken = default)
    {
        ApiVolume? volume = await _client.GetVolumeAsync(cancellationToken);
        return (int)(volume?.Value ?? 0);
    }
}
