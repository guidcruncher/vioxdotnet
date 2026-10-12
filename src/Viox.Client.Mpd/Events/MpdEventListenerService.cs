using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Viox.Client.Mpd.Services;
using Viox.Core.Events;
using Viox.Core.Models;

namespace Viox.Client.Mpd.Events;

/// <summary>
/// Background service that listens for events from the Music Player Daemon (MPD) and publishes media events.
/// </summary>
public sealed class MpdEventListenerService : BackgroundService
{
    private readonly IMpdClientIdle _mpdClient;
    private readonly IMediaEventService _eventService;
    private readonly ILogger<MpdEventListenerService> _logger;

    private MediaMetaData? _lastTrack;
    private double _lastPosition;
    private bool _lastPlaying;

    private bool _lastShuffle;
    private bool _lastRepeat;

    /// <summary>
    /// Initializes a new instance of the <see cref="MpdEventListenerService"/> class.
    /// </summary>
    /// <param name="mpdClient">The MPD client.</param>
    /// <param name="eventService the media event service.">The media event service.</param>
    /// <param name="logger">The logger.</param>
    public MpdEventListenerService(
        IMpdClientIdle mpdClient,
        IMediaEventService eventService,
        ILogger<MpdEventListenerService> logger)
    {
        _mpdClient = mpdClient;
        _eventService = eventService;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Starting MPD event listener...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _mpdClient.ConnectAsync(
                    stoppingToken);

                string response =
                    await _mpdClient.SendCommandAsync(
                        "idle player mixer playlist options update",
                        stoppingToken);

                string? eventLine =
                    response.Split(
                            '\n',
                            StringSplitOptions.RemoveEmptyEntries)
                        .FirstOrDefault();

                if (eventLine is null)
                    continue;

                if (!eventLine.StartsWith(
                        "changed:",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string mpdEvent =
                    eventLine["changed:".Length..].Trim();

                await HandleEventAsync(
                    mpdEvent,
                    stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "MPD listener failed, restarting...");

                await Task.Delay(
                    1000,
                    stoppingToken);
            }
        }

        _logger.LogInformation(
            "MPD event listener stopped.");
    }

    private async Task HandleEventAsync(
        string mpdEvent,
        CancellationToken ct)
    {
        string status =
            await _mpdClient.SendCommandAsync(
                "status",
                ct);

        string currentSong =
            await _mpdClient.SendCommandAsync(
                "currentsong",
                ct);

        PlaybackState state =
            ParsePlaybackState(
                status,
                currentSong);

        await PublishPlaybackEventsAsync(
            state,
            ct);

        switch (mpdEvent)
        {
            case "mixer":
                await PublishEventAsync(
                    MediaEventType.VolumeChanged,
                    state,
                    ct);
                break;

            case "playlist":
                await PublishEventAsync(
                    MediaEventType.QueueChanged,
                    state,
                    ct);
                break;

            case "options":
                await PublishOptionEventsAsync(
                    status,
                    state,
                    ct);
                break;
        }

        await DetectTrackFinishedAsync(
            state,
            ct);

        await DetectSeekAsync(
            state,
            ct);

        await _eventService.PublishPlaybackStatusAsync(
            state,
            ct);

        _lastTrack = state.Track;
        _lastPosition = state.Position;
        _lastPlaying = state.Playing;
    }

    private async Task PublishPlaybackEventsAsync(
        PlaybackState state,
        CancellationToken ct)
    {
        bool trackChanged =
            _lastTrack is not null &&
            state.Track is not null &&
            !string.Equals(
                _lastTrack.Url,
                state.Track.Url,
                StringComparison.OrdinalIgnoreCase);

        if (trackChanged)
        {
            await PublishEventAsync(
                MediaEventType.TrackChanged,
                state,
                ct);

            await PublishEventAsync(
                MediaEventType.TrackStarted,
                state,
                ct);
        }

        if (!_lastPlaying && state.Playing)
        {
            await PublishEventAsync(
                MediaEventType.PlaybackStarted,
                state,
                ct);
        }

        if (_lastPlaying && !state.Playing)
        {
            await PublishEventAsync(
                MediaEventType.PlaybackPaused,
                state,
                ct);
        }
    }

    private async Task DetectTrackFinishedAsync(
        PlaybackState state,
        CancellationToken ct)
    {
        if (_lastTrack is null)
            return;

        bool trackChanged =
            state.Track?.Url is not null &&
            !string.Equals(
                _lastTrack.Url,
                state.Track.Url,
                StringComparison.OrdinalIgnoreCase);

        bool elapsedReset =
            state.Position < 1 &&
            _lastPosition > 3;

        if (trackChanged &&
            elapsedReset &&
            state.Playing)
        {
            await PublishEventAsync(
                MediaEventType.TrackFinished,
                state,
                _lastTrack,
                ct);
        }
    }

    private async Task DetectSeekAsync(
        PlaybackState state,
        CancellationToken ct)
    {
        if (_lastTrack?.Url != state.Track?.Url)
            return;

        double delta =
            Math.Abs(
                state.Position -
                _lastPosition);

        if (delta > 5)
        {
            await PublishEventAsync(
                MediaEventType.Seeked,
                state,
                ct);
        }
    }

    private async Task PublishOptionEventsAsync(
        string status,
        PlaybackState state,
        CancellationToken ct)
    {
        bool shuffle =
            ParseFlag(status, "random");

        bool repeat =
            ParseFlag(status, "repeat");

        if (shuffle != _lastShuffle)
        {
            await PublishEventAsync(
                MediaEventType.ShuffleChanged,
                state,
                ct);

            _lastShuffle = shuffle;
        }

        if (repeat != _lastRepeat)
        {
            await PublishEventAsync(
                MediaEventType.RepeatChanged,
                state,
                ct);

            _lastRepeat = repeat;
        }
    }

    private async Task PublishEventAsync(
        MediaEventType type,
        PlaybackState state,
        CancellationToken ct)
    {
        await PublishEventAsync(
            type,
            state,
            state.Track,
            ct);
    }

    private async Task PublishEventAsync(
        MediaEventType type,
        PlaybackState state,
        MediaMetaData? track,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Publishing media event: {Event}",
            type);

        MediaEvent mediaEvent = new(
            Type: type,
            Backend: state.ActiveBackend,
            Track: track,
            Data: new
            {
                Position = state.Position,
                Playing = state.Playing,
                IsLive = state.IsLive
            },
            Timestamp: DateTimeOffset.UtcNow);

        await _eventService.PublishMediaEventAsync(
            mediaEvent,
            ct);
    }

    private static bool ParseFlag(
        string status,
        string property)
    {
        using var sr =
            new StringReader(status);

        string? line;

        while ((line = sr.ReadLine()) != null)
        {
            if (!line.StartsWith(
                    property + ":",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return line[(property.Length + 1)..]
                .Trim() == "1";
        }

        return false;
    }

    private PlaybackState ParsePlaybackState(
        string status,
        string currentSong)
    {
        var track =
            ParseMetadata(currentSong);

        double position = 0;
        bool playing = false;

        using var sr =
            new StringReader(status);

        string? line;

        while ((line = sr.ReadLine()) != null)
        {
            if (line.StartsWith(
                    "elapsed:",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (double.TryParse(
                        line[8..].Trim(),
                        out var pos))
                {
                    position = pos;
                }
            }

            if (line.StartsWith(
                    "state:",
                    StringComparison.OrdinalIgnoreCase))
            {
                var state =
                    line["state:".Length..].Trim();

                playing =
                    state.Equals(
                        "play",
                        StringComparison.OrdinalIgnoreCase);
            }
        }

        return new PlaybackState
        {
            ActiveBackend = "mpd",
            Track = track,
            Position = position,
            Playing = playing,
            IsLive = false
        };
    }

    private MediaMetaData ParseMetadata(
        string response)
    {
        string title = "";
        string artist = "";
        string album = "";
        string url = "";

        using var sr =
            new StringReader(response);

        string? line;

        while ((line = sr.ReadLine()) != null)
        {
            int idx = line.IndexOf(':');

            if (idx <= 0)
                continue;

            string key =
                line[..idx].Trim();

            string value =
                line[(idx + 1)..].Trim();

            switch (key.ToLowerInvariant())
            {
                case "title":
                    title = value;
                    break;

                case "artist":
                    artist = value;
                    break;

                case "album":
                    album = value;
                    break;

                case "file":
                    url = value;
                    break;
            }
        }

        return new MediaMetaData
        {
            Title = title,
            Artist = artist,
            Album = album,
            Url = url,
            ImageUrl = string.Empty
        };
    }
}
