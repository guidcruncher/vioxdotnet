using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Viox.Client.Mpd.Services;
using Viox.Core.Models;

namespace Viox.Client.Mpd.Events;

public sealed class MpdEventWorker : BackgroundService
{
    private readonly IMpdClientIdle _mpdClient;
    private readonly IMpdEventService _eventService;
    private readonly ILogger<MpdEventWorker> _logger;

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

    public MpdEventWorker(
        IMpdClientIdle mpdClient,
        IMpdEventService eventService,
        ILogger<MpdEventWorker> logger)
    {
        _mpdClient = mpdClient;
        _eventService = eventService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting MPD event listener worker...");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _mpdClient.ConnectAsync(stoppingToken);
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
        _logger.LogInformation("MPD event listener worker stopped.");
    }

    private async Task HandleSubsystemAsync(string subsystem, CancellationToken ct)
    {
        string status = await _mpdClient.SendCommandAsync("status", ct);
        string currentSong = await _mpdClient.SendCommandAsync("currentsong", ct);
        var newState = ParsePlaybackState(status, currentSong);

        switch (subsystem)
        {
            case "player":
                await HandlePlayerEventsAsync(newState, ct);
                break;
            case "mixer":
                HandleMixerEvents(status);
                break;
            case "options":
                HandleOptionEvents(status);
                break;
            case "playlist":
                break;
        }

        _eventService.PublishStatus(this, newState);
        _prevState = newState;
    }

    private async Task HandlePlayerEventsAsync(PlaybackState newState, CancellationToken ct)
    {
        var prev = _prevState;
        bool trackChanged =
            prev.Track?.Url is not null &&
            newState.Track?.Url is not null &&
            !string.Equals(prev.Track.Url, newState.Track.Url, StringComparison.OrdinalIgnoreCase);
        bool wasPlaying = prev.Playing;
        bool isPlaying = newState.Playing;

        if (trackChanged && wasPlaying)
        {
            _logger.LogInformation("Raising MPD event: {EventType}", "TrackFinished");
            _eventService.PublishTrackFinished(this, new MpdTrackFinishedEvent(prev.Track));

            _logger.LogInformation("Raising MPD event: {EventType}", "TrackStarted");
            _eventService.PublishTrackStarted(this, new MpdTrackStartedEvent(newState.Track));
        }
        else if (!trackChanged && isPlaying && !wasPlaying)
        {
            _logger.LogInformation("Raising MPD event: {EventType}", "PlaybackStarted");
            _eventService.PublishPlaybackStarted(this, new MpdPlaybackStartedEvent(newState.Position, newState.Playing));
        }

        if (wasPlaying && !isPlaying)
        {
            _logger.LogInformation("Raising MPD event: {EventType}", "PlaybackPaused");
            _eventService.PublishPlaybackPaused(this, new MpdPlaybackPausedEvent(newState.Position));
        }
        else if (!wasPlaying && isPlaying)
        {
            _logger.LogInformation("Raising MPD event: {EventType}", "PlaybackStarted");
            _eventService.PublishPlaybackStarted(this, new MpdPlaybackStartedEvent(newState.Position, newState.Playing));
        }

        if (prev.Track?.Url == newState.Track?.Url)
        {
            double delta = Math.Abs(newState.Position - prev.Position);
            if (delta > 3)
            {
                _logger.LogInformation("Raising MPD event: {EventType}", "Seeked");
                _eventService.PublishSeeked(this, new MpdSeekedEvent(newState.Position, newState.Track?.Duration ?? 0));
            }
        }

        await Task.CompletedTask;
    }

    private void HandleMixerEvents(string status)
    {
        int volume = ParseInt(status, "volume");
        if (volume != _prevVolume)
        {
            _prevVolume = volume;
            _logger.LogInformation("Raising MPD event: {EventType}", "VolumeChanged");
            _eventService.PublishVolume(this, new MpdVolumeEvent(volume, 100));
        }
    }

    private void HandleOptionEvents(string status)
    {
        bool shuffle = ParseFlag(status, "random");
        bool repeat = ParseFlag(status, "repeat");

        if (shuffle != _prevShuffle)
        {
            _prevShuffle = shuffle;
            _logger.LogInformation("Raising MPD event: {EventType}", "ShuffleChanged");
            _eventService.PublishShuffleChanged(this, new MpdShuffleChangedEvent(shuffle));
        }

        if (repeat != _prevRepeat)
        {
            _prevRepeat = repeat;
            _logger.LogInformation("Raising MPD event: {EventType}", "RepeatChanged");
            _eventService.PublishRepeatChanged(this, new MpdRepeatChangedEvent(repeat));
        }
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
        var track = ParseMetadata(currentSong);
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
            Track = track,
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
