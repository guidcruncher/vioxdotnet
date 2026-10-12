using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Viox.Client.Mpd.Services;
using Viox.Core.Events;
using Viox.Core.Models;

namespace Viox.Client.Mpd.Events;

/// <summary>
/// Listens for MPD events and publishes corresponding media events.
/// </summary>
public sealed class MpdEventListenerService : BackgroundService
{
    private readonly IMpdClientIdle _mpdClient;
    private readonly IMediaEventService _eventService;
    private readonly ILogger<MpdEventListenerService> _logger;

    // Previous state snapshot
    private PlaybackState _prevState = new()
    {
        ActiveBackend = "mpd",
        Track = null,
        Position = 0,
        Playing = false,
        IsLive = false
    };

    private bool _prevShuffle;
    private bool _prevRepeat;
    private int _prevVolume;

    public MpdEventListenerService(
        IMpdClientIdle mpdClient,
        IMediaEventService eventService,
        ILogger<MpdEventListenerService> logger)
    {
        _mpdClient = mpdClient;
        _eventService = eventService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting MPD event listener...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _mpdClient.ConnectAsync(stoppingToken);

                // Listen only to subsystems we care about
                string response = await _mpdClient.SendCommandAsync(
                    "idle player mixer playlist options",
                    stoppingToken);

                string? eventLine = response
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault();

                if (eventLine is null ||
                    !eventLine.StartsWith("changed:", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string subsystem = eventLine["changed:".Length..].Trim();

                await HandleSubsystemAsync(subsystem, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MPD listener failed, restarting...");
                await Task.Delay(1000, stoppingToken);
            }
        }

        _logger.LogInformation("MPD event listener stopped.");
    }

    private async Task HandleSubsystemAsync(string subsystem, CancellationToken ct)
    {
        // Always refresh status + currentsong for a consistent snapshot
        string status = await _mpdClient.SendCommandAsync("status", ct);
        string currentSong = await _mpdClient.SendCommandAsync("currentsong", ct);

        var newState = ParsePlaybackState(status, currentSong);

        // Detect events based on subsystem + previous vs current snapshot
        switch (subsystem)
        {
            case "player":
                await HandlePlayerEventsAsync(status, newState, ct);
                break;

            case "mixer":
                await HandleMixerEventsAsync(status, newState, ct);
                break;

            case "options":
                await HandleOptionEventsAsync(status, newState, ct);
                break;

            case "playlist":
                // await PublishEventAsync(MediaEventType.QueueChanged, newState, ct);
                break;
        }

        // Always publish current playback status
        await _eventService.PublishPlaybackStatusAsync(newState, ct);

        // Update previous snapshot
        _prevState = newState;
    }

    // PLAYER: track start, track finish, seek, pause/resume
    private async Task HandlePlayerEventsAsync(
        string status,
        PlaybackState newState,
        CancellationToken ct)
    {
        var prev = _prevState;

        bool trackChanged =
            prev.Track?.Url is not null &&
            newState.Track?.Url is not null &&
            !string.Equals(prev.Track.Url, newState.Track.Url, StringComparison.OrdinalIgnoreCase);

        bool wasPlaying = prev.Playing;
        bool isPlaying = newState.Playing;

        // Track finished: old track was playing, now a different track is active
        if (trackChanged && wasPlaying)
        {
            await PublishEventAsync(
                MediaEventType.TrackFinished,
                newState,
                prev.Track,
                ct);

            await PublishEventAsync(
                MediaEventType.TrackStarted,
                newState,
                newState.Track,
                ct);
        }
        else if (!trackChanged && isPlaying && !wasPlaying)
        {
            // Resume or start on same track
            await PublishEventAsync(MediaEventType.PlaybackStarted, newState, ct);
        }

        // Pause / resume
        if (wasPlaying && !isPlaying)
        {
            await PublishEventAsync(MediaEventType.PlaybackPaused, newState, ct);
        }
        else if (!wasPlaying && isPlaying)
        {
            await PublishEventAsync(MediaEventType.PlaybackStarted, newState, ct);
        }

        // Seek: same track, position jump
        if (prev.Track?.Url == newState.Track?.Url)
        {
            double delta = Math.Abs(newState.Position - prev.Position);

            // Threshold in seconds
            if (delta > 3)
            {
                await PublishEventAsync(MediaEventType.Seeked, newState, ct);
            }
        }
    }

    // MIXER: volume changes
    private async Task HandleMixerEventsAsync(
        string status,
        PlaybackState newState,
        CancellationToken ct)
    {
        int volume = ParseInt(status, "volume");

        if (volume != _prevVolume)
        {
            _prevVolume = volume;

            await PublishEventAsync(
                MediaEventType.VolumeChanged,
                newState,
                ct);
        }
    }

    // OPTIONS: shuffle / repeat changes
    private async Task HandleOptionEventsAsync(
        string status,
        PlaybackState newState,
        CancellationToken ct)
    {
        bool shuffle = ParseFlag(status, "random");
        bool repeat = ParseFlag(status, "repeat");

        if (shuffle != _prevShuffle)
        {
            _prevShuffle = shuffle;
            await PublishEventAsync(MediaEventType.ShuffleChanged, newState, ct);
        }

        if (repeat != _prevRepeat)
        {
            _prevRepeat = repeat;
            await PublishEventAsync(MediaEventType.RepeatChanged, newState, ct);
        }
    }

    private async Task PublishEventAsync(
        MediaEventType type,
        PlaybackState state,
        CancellationToken ct)
    {
        await PublishEventAsync(type, state, state.Track, ct);
    }

    private async Task PublishEventAsync(
        MediaEventType type,
        PlaybackState state,
        MediaMetaData? track,
        CancellationToken ct)
    {
        _logger.LogInformation("Publishing media event: {Event}", type);

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

        await _eventService.PublishMediaEventAsync(mediaEvent, ct);
    }

    private static bool ParseFlag(string status, string property)
    {
        using var sr = new StringReader(status);
        string? line;

        while ((line = sr.ReadLine()) != null)
        {
            if (!line.StartsWith(property + ":", StringComparison.OrdinalIgnoreCase))
                continue;

            return line[(property.Length + 1)..].Trim() == "1";
        }

        return false;
    }

    private static int ParseInt(string status, string property)
    {
        using var sr = new StringReader(status);
        string? line;

        while ((line = sr.ReadLine()) != null)
        {
            if (!line.StartsWith(property + ":", StringComparison.OrdinalIgnoreCase))
                continue;

            if (int.TryParse(line[(property.Length + 1)..].Trim(), out var value))
                return value;
        }

        return 0;
    }

    private PlaybackState ParsePlaybackState(string status, string currentSong)
    {
        //var track = ParseMetadata(currentSong);

        double position = 0;
        bool playing = false;

        using var sr = new StringReader(status);
        string? line;

        while ((line = sr.ReadLine()) != null)
        {
            if (line.StartsWith("elapsed:", StringComparison.OrdinalIgnoreCase))
            {
                if (double.TryParse(line["elapsed:".Length..].Trim(), out var pos))
                    position = pos;
            }

            if (line.StartsWith("state:", StringComparison.OrdinalIgnoreCase))
            {
                var state = line["state:".Length..].Trim();
                playing = state.Equals("play", StringComparison.OrdinalIgnoreCase);
            }
        }

        return new PlaybackState
        {
            ActiveBackend = "mpd",
            Track = null,
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
            if (idx <= 0)
                continue;

            string key = line[..idx].Trim();
            string value = line[(idx + 1)..].Trim();

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
