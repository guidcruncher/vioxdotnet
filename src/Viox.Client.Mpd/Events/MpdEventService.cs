using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Viox.Client.Mpd.Services;
using Viox.Core.Models;

namespace Viox.Client.Mpd.Events;

/// <summary>
/// Listens for MPD events and raises them via strongly typed C# events.
/// </summary>
public sealed class MpdEventService : BackgroundService, IMpdEventService
{
    private readonly IMpdClientIdle _mpdClient;
    private readonly ILogger<MpdEventService> _logger;

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

    public event EventHandler<MpdEvent>? OnEventReceived;
    public event EventHandler<MpdTrackStartedEvent>? OnTrackStartedReceived;
    public event EventHandler<MpdTrackFinishedEvent>? OnTrackFinishedReceived;
    public event EventHandler<MpdPlaybackStartedEvent>? OnPlaybackStartedReceived;
    public event EventHandler<MpdPlaybackPausedEvent>? OnPlaybackPausedReceived;
    public event EventHandler<MpdSeekedEvent>? OnSeekedReceived;
    public event EventHandler<MpdVolumeEvent>? OnVolumeReceived;
    public event EventHandler<MpdShuffleChangedEvent>? OnShuffleChangedReceived;
    public event EventHandler<MpdRepeatChangedEvent>? OnRepeatChangedReceived;
    public event EventHandler<PlaybackState>? OnPlaybackStatusUpdated;

    public MpdEventService(
        IMpdClientIdle mpdClient,
        ILogger<MpdEventService> logger)
    {
        _mpdClient = mpdClient;
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
        string status = await _mpdClient.SendCommandAsync("status", ct);
        string currentSong = await _mpdClient.SendCommandAsync("currentsong", ct);

        var newState = ParsePlaybackState(status, currentSong);

        switch (subsystem)
        {
            case "player":
                await HandlePlayerEventsAsync(newState, ct);
                break;

            case "mixer":
                HandleMixerEvents(status, newState);
                break;

            case "options":
                HandleOptionEvents(status, newState);
                break;

            case "playlist":
                break;
        }

        OnPlaybackStatusUpdated?.Invoke(this, newState);
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
            RaiseEvent(new MpdTrackFinishedEvent(prev.Track), OnTrackFinishedReceived);
            RaiseEvent(new MpdTrackStartedEvent(newState.Track), OnTrackStartedReceived);
        }
        else if (!trackChanged && isPlaying && !wasPlaying)
        {
            RaiseEvent(new MpdPlaybackStartedEvent(newState.Position, newState.Playing), OnPlaybackStartedReceived);
        }

        if (wasPlaying && !isPlaying)
        {
            RaiseEvent(new MpdPlaybackPausedEvent(newState.Position), OnPlaybackPausedReceived);
        }
        else if (!wasPlaying && isPlaying)
        {
            RaiseEvent(new MpdPlaybackStartedEvent(newState.Position, newState.Playing), OnPlaybackStartedReceived);
        }

        if (prev.Track?.Url == newState.Track?.Url)
        {
            double delta = Math.Abs(newState.Position - prev.Position);

            if (delta > 3)
            {
                RaiseEvent(new MpdSeekedEvent(newState.Position, newState.Track?.Duration ?? 0), OnSeekedReceived);
            }
        }

        await Task.CompletedTask;
    }

    private void HandleMixerEvents(string status, PlaybackState newState)
    {
        int volume = ParseInt(status, "volume");

        if (volume != _prevVolume)
        {
            _prevVolume = volume;
            RaiseEvent(new MpdVolumeEvent(volume, 100), OnVolumeReceived);
        }
    }

    private void HandleOptionEvents(string status, PlaybackState newState)
    {
        bool shuffle = ParseFlag(status, "random");
        bool repeat = ParseFlag(status, "repeat");

        if (shuffle != _prevShuffle)
        {
            _prevShuffle = shuffle;
            RaiseEvent(new MpdShuffleChangedEvent(shuffle), OnShuffleChangedReceived);
        }

        if (repeat != _prevRepeat)
        {
            _prevRepeat = repeat;
            RaiseEvent(new MpdRepeatChangedEvent(repeat), OnRepeatChangedReceived);
        }
    }

    private void RaiseEvent<T>(T mpdEvent, EventHandler<T>? handler) where T : MpdEvent
    {
        _logger.LogInformation("Raising MPD event: {EventType}", mpdEvent.Type);
        OnEventReceived?.Invoke(this, mpdEvent);
        handler?.Invoke(this, mpdEvent);
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
