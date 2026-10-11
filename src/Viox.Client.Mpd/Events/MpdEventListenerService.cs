using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Viox.Client.Mpd.Services;
using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Client.Mpd.Events;

/// <summary>
/// Background service that listens for MPD events using the MPD <c>idle</c> command
/// and publishes playback and media actions through <see cref="IMediaEventService"/>.
/// </summary>
public sealed class MpdEventListenerService : BackgroundService
{
    private readonly IMpdClient _mpdClient;
    private readonly IMediaEventService _eventService;
    private readonly ILogger<MpdEventListenerService> _logger;

    private MediaMetaData? _lastTrack = null;
    private double _lastPosition = 0;
    private bool _lastPlaying = false;

    /// <summary>
    /// Creates a new instance of the MPD event listener service.
    /// </summary>
    /// <param name="mpdClient">The MPD client used to communicate with the MPD server.</param>
    /// <param name="eventService">The event publishing service for media and playback events.</param>
    /// <param name="logger">Logger instance for diagnostic output.</param>
    public MpdEventListenerService(
        IMpdClient mpdClient,
        IMediaEventService eventService,
        ILogger<MpdEventListenerService> logger)
    {
        _mpdClient = mpdClient;
        _eventService = eventService;
        _logger = logger;
    }

    /// <summary>
    /// Executes the background MPD event loop, subscribing to MPD change notifications
    /// and publishing structured events to the application event pipeline.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token used to stop the background service.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting MPD event listener...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _mpdClient.ConnectAsync(stoppingToken);

                string response = await _mpdClient.SendCommandAsync(
                    "idle player mixer playlist options update",
                    stoppingToken);

                string? eventLine = response
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault();

                if (eventLine is null)
                    continue;

                if (eventLine.StartsWith("changed:", StringComparison.OrdinalIgnoreCase))
                {
                    string eventType = eventLine.Substring("changed:".Length).Trim();
                    await HandleEventAsync(eventType, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MPD event listener encountered an error. Restarting...");
                await Task.Delay(1000, stoppingToken);
            }
        }

        _logger.LogInformation("MPD event listener stopped.");
    }

    private async Task HandleEventAsync(string eventType, CancellationToken cancellationToken)
    {
        _logger.LogInformation("MPD event received: {Event}", eventType);

        string status = await _mpdClient.SendCommandAsync("status", cancellationToken);
        string currentSong = await _mpdClient.SendCommandAsync("currentsong", cancellationToken);

        PlaybackState playbackState = ParsePlaybackState(status, currentSong);

        await DetectTrackFinishedAsync(playbackState, cancellationToken);

        await _eventService.PublishMediaActionAsync(eventType, playbackState.Track, cancellationToken);
        await _eventService.PublishPlaybackStatusAsync(playbackState, cancellationToken);

        _lastTrack = playbackState.Track;
        _lastPosition = playbackState.Position;
        _lastPlaying = playbackState.Playing;
    }

    private async Task DetectTrackFinishedAsync(
        PlaybackState newState,
        CancellationToken ct)
    {
        if (_lastTrack is null)
            return;

        var newTrack = newState.Track;

        bool trackChanged =
            newTrack?.Url is not null &&
            _lastTrack.Url is not null &&
            !string.Equals(newTrack.Url, _lastTrack.Url, StringComparison.OrdinalIgnoreCase);

        bool elapsedReset =
            newState.Position < 1 &&
            _lastPosition > 3;

        bool stillPlaying = newState.Playing;

        if (trackChanged && elapsedReset && stillPlaying)
        {
            _logger.LogInformation("Detected track finished: {Title}", _lastTrack.Title);

            await _eventService.PublishMediaActionAsync(
                "track_finished",
                _lastTrack,
                ct);
        }
    }

    private PlaybackState ParsePlaybackState(string status, string currentSong)
    {
        var meta = ParseMetadata(currentSong);

        double position = 0;
        bool playing = false;

        using var sr = new StringReader(status);
        string? line;
        while ((line = sr.ReadLine()) != null)
        {
            if (line.StartsWith("elapsed:", StringComparison.OrdinalIgnoreCase))
            {
                if (double.TryParse(line.Substring(8).Trim(), out double pos))
                    position = pos;
            }

            if (line.StartsWith("state:", StringComparison.OrdinalIgnoreCase))
            {
                string state = line.Substring("state:".Length).Trim();
                playing = state.Equals("play", StringComparison.OrdinalIgnoreCase);
            }
        }

        return new PlaybackState
        {
            ActiveBackend = "mpd",
            Track = meta,
            Position = position,
            Playing = playing,
            IsLive = false
        };
    }

    private MediaMetaData ParseMetadata(string response)
    {
        string title = "";
        string artist = "";
        string album = "";
        string url = "";

        using var sr = new StringReader(response);
        string? line;
        while ((line = sr.ReadLine()) != null)
        {
            int idx = line.IndexOf(':');
            if (idx <= 0) continue;

            string key = line[..idx].Trim();
            string value = line[(idx + 1)..].Trim();

            switch (key.ToLowerInvariant())
            {
                case "title": title = value; break;
                case "artist": artist = value; break;
                case "album": album = value; break;
                case "file": url = value; break;
            }
        }

        return new MediaMetaData
        {
            Title = title,
            Artist = artist,
            Album = album,
            Url = url,
            ImageUrl = ""
        };
    }
}
