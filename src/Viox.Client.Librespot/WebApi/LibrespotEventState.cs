using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

using Viox.Client.Librespot.Events;
using Viox.Core.Events;
using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Client.Librespot.WebApi;

/// <summary>
/// Thread-safe state store capturing every WebSocket event type emitted by go-librespot.
/// </summary>
public sealed class LibrespotEventState
{
    private readonly ConcurrentQueue<LibrespotEvent> _recentEvents = new();
    private readonly IMediaEventService _eventService;
    private readonly ILogger<LibrespotEventState> _logger;

    private readonly Timer _positionTimer;
    private const int MaxStoredEvents = 100;

    public bool IsActive { get; private set; }
    public bool IsPlaying { get; private set; }
    public MetadataEvent? CurrentTrack { get; private set; }
    public WillPlayEvent? PendingTrack { get; private set; }

    public string CurrentContextUri { get; private set; } = string.Empty;
    public string CurrentTrackUri { get; private set; } = string.Empty;
    public string LastPlayOrigin { get; private set; } = string.Empty;

    public long CurrentPositionMs { get; private set; }
    public long CurrentDurationMs { get; private set; }

    public uint Volume { get; private set; }
    public uint MaxVolume { get; private set; }

    public bool ShuffleContext { get; private set; }
    public bool RepeatContext { get; private set; }
    public bool RepeatTrack { get; private set; }

    public LibrespotEventState(
        ILogger<LibrespotEventState> logger,
        IMediaEventService eventService)
    {
        _logger = logger;
        _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));

        // Poll every 500ms for natural track end
        _positionTimer = new Timer(CheckTrackEnd, null, 500, 500);
    }

    private PlaybackState GetPlaybackState(MetadataEvent metadata)
    {
        MediaUri? uri = MediaUriParser.ParseMediaUriValue(metadata.Uri);

        MediaMetaData track = new()
        {
            Uri = uri,
            Album = metadata.AlbumName,
            Title = metadata.Name,
            Artist = string.Join(", ", metadata.ArtistNames),
            Url = string.Empty,
            ImageUrl = string.IsNullOrEmpty(metadata.AlbumCoverUrl) ? string.Empty : metadata.AlbumCoverUrl,
            Duration = metadata.Duration / 1000d
        };

        return new PlaybackState
        {
            ActiveBackend = "librespot",
            Track = track,
            Position = metadata.Position / 1000d,
            Playing = IsPlaying,
            IsLive = metadata.Duration <= 0
        };
    }

    private async Task PublishEventAsync(MediaEventType type, PlaybackState state)
    {
        MediaEvent mediaEvent = new(
            Type: type,
            Backend: state.ActiveBackend,
            Track: state.Track,
            Data: new
            {
                Position = state.Position,
                Playing = state.Playing,
                IsLive = state.IsLive
            },
            Timestamp: DateTimeOffset.UtcNow);

        await _eventService.PublishMediaEventAsync(mediaEvent, CancellationToken.None);
    }

    // 🔥 NATURAL END DETECTION (polling)
    private void CheckTrackEnd(object? _)
    {
        if (!IsPlaying || CurrentTrack == null)
            return;

        if (CurrentDurationMs <= 0)
            return; // live stream

        // 500ms tolerance
        if (CurrentPositionMs >= CurrentDurationMs - 500)
        {
            FireTrackFinished(CurrentTrack);
        }
    }

    public void UpdateState(LibrespotEvent @event)
    {
        switch (@event)
        {
            case ActiveEvent:
                IsActive = true;
                break;

            case InactiveEvent:
                IsActive = false;
                break;

            case MetadataEvent metadata:
                {
                    CurrentTrack = metadata;
                    CurrentContextUri = metadata.ContextUri;
                    CurrentTrackUri = metadata.Uri;
                    CurrentPositionMs = metadata.Position;
                    CurrentDurationMs = metadata.Duration;

                    var statusState = GetPlaybackState(metadata);

                    Task.Run(async () =>
                    {
                        await _eventService.PublishPlaybackStatusAsync(statusState, CancellationToken.None);
                        await PublishEventAsync(MediaEventType.Status, statusState);
                    }).GetAwaiter().GetResult();

                    break;
                }

            case WillPlayEvent willPlay:
                {
                    PendingTrack = willPlay;
                    CurrentContextUri = willPlay.ContextUri;
                    CurrentTrackUri = willPlay.Uri;
                    LastPlayOrigin = willPlay.PlayOrigin;
                    break;
                }

            case PlayingEvent playing:
                {
                    IsPlaying = true;
                    CurrentContextUri = playing.ContextUri;
                    CurrentTrackUri = playing.Uri;
                    LastPlayOrigin = playing.PlayOrigin;

                    if (CurrentTrack != null)
                    {
                        var playbackState = GetPlaybackState(CurrentTrack);

                        Task.Run(async () =>
                        {
                            await PublishEventAsync(MediaEventType.PlaybackStarted, playbackState);
                        }).GetAwaiter().GetResult();
                    }

                    break;
                }

            case NotPlayingEvent notPlaying:
                {
                    IsPlaying = false;
                    CurrentContextUri = notPlaying.ContextUri;
                    CurrentTrackUri = notPlaying.Uri;
                    LastPlayOrigin = notPlaying.PlayOrigin;

                    if (CurrentTrack != null)
                        FireTrackFinished(CurrentTrack);

                    break;
                }

            case PausedEvent paused:
                {
                    IsPlaying = false;
                    CurrentContextUri = paused.ContextUri;
                    CurrentTrackUri = paused.Uri;
                    LastPlayOrigin = paused.PlayOrigin;

                    if (CurrentTrack != null)
                    {
                        var pauseState = GetPlaybackState(CurrentTrack);

                        Task.Run(async () =>
                        {
                            await PublishEventAsync(MediaEventType.PlaybackPaused, pauseState);
                        }).GetAwaiter().GetResult();
                    }

                    break;
                }

            case StoppedEvent stopped:
                {
                    IsPlaying = false;
                    LastPlayOrigin = stopped.PlayOrigin;

                    if (CurrentTrack != null)
                        FireTrackFinished(CurrentTrack);

                    break;
                }

            case SeekEvent seek:
                {
                    CurrentPositionMs = seek.Position;
                    CurrentDurationMs = seek.Duration;
                    CurrentContextUri = seek.ContextUri;
                    CurrentTrackUri = seek.Uri;
                    LastPlayOrigin = seek.PlayOrigin;

                    if (CurrentTrack != null)
                    {
                        var seekState = GetPlaybackState(CurrentTrack);

                        Task.Run(async () =>
                        {
                            await PublishEventAsync(MediaEventType.Seeked, seekState);
                        }).GetAwaiter().GetResult();
                    }

                    break;
                }

            case VolumeEvent volume:
                {
                    Volume = volume.Value;
                    MaxVolume = volume.Max;

                    if (CurrentTrack != null)
                    {
                        var volumeState = GetPlaybackState(CurrentTrack);

                        Task.Run(async () =>
                        {
                            await PublishEventAsync(MediaEventType.VolumeChanged, volumeState);
                        }).GetAwaiter().GetResult();
                    }

                    break;
                }

            case ShuffleContextEvent shuffle:
                {
                    ShuffleContext = shuffle.Value;

                    if (CurrentTrack != null)
                    {
                        var shuffleState = GetPlaybackState(CurrentTrack);

                        Task.Run(async () =>
                        {
                            await PublishEventAsync(MediaEventType.ShuffleChanged, shuffleState);
                        }).GetAwaiter().GetResult();
                    }

                    break;
                }

            case RepeatContextEvent repeatContext:
                {
                    RepeatContext = repeatContext.Value;

                    if (CurrentTrack != null)
                    {
                        var repeatState = GetPlaybackState(CurrentTrack);

                        Task.Run(async () =>
                        {
                            await PublishEventAsync(MediaEventType.RepeatChanged, repeatState);
                        }).GetAwaiter().GetResult();
                    }

                    break;
                }

            case RepeatTrackEvent repeatTrack:
                RepeatTrack = repeatTrack.Value;
                break;

            case UnknownLibrespotEvent:
                break;
        }

        _recentEvents.Enqueue(@event);
        while (_recentEvents.Count > MaxStoredEvents)
            _recentEvents.TryDequeue(out _);
    }

    private void FireTrackFinished(MetadataEvent track)
    {
        PlaybackState state = GetPlaybackState(track);
        _logger.LogWarning("*** Track finished: {TrackUri}", track.Uri);

        Task.Run(async () =>
        {
            await PublishEventAsync(MediaEventType.TrackFinished, state);
        }).GetAwaiter().GetResult();
    }

    public IReadOnlyCollection<LibrespotEvent> GetRecentEvents()
    {
        return _recentEvents.ToArray();
    }
}
