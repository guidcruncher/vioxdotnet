using System.Collections.Concurrent;

using Viox.Client.Librespot.Events;
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

    public LibrespotEventState(IMediaEventService eventService)
    {
        _eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
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
            Duration = Convert.ToDouble(metadata.Duration / 1000L)
        };

        PlaybackState state = new()
        {
            ActiveBackend = "librespot",
            Track = track,
            Position = Convert.ToDouble(metadata.Position / 1000L),
            Playing = true,
            IsLive = metadata.Duration <= 0
        };

        return state;
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
                CurrentTrack = metadata;
                CurrentContextUri = metadata.ContextUri;
                CurrentTrackUri = metadata.Uri;
                CurrentPositionMs = metadata.Position;
                CurrentDurationMs = metadata.Duration;

                PlaybackState state = GetPlaybackState(metadata);
                Task.Run(async () => await _eventService.PublishPlaybackStatusAsync(state)).GetAwaiter().GetResult();
                break;
            case WillPlayEvent willPlay:
                PendingTrack = willPlay;
                CurrentContextUri = willPlay.ContextUri;
                CurrentTrackUri = willPlay.Uri;
                LastPlayOrigin = willPlay.PlayOrigin;
                break;
            case PlayingEvent playing:
                IsPlaying = true;
                CurrentContextUri = playing.ContextUri;
                CurrentTrackUri = playing.Uri;
                LastPlayOrigin = playing.PlayOrigin;
                break;
            case NotPlayingEvent notPlaying:
                IsPlaying = false;
                CurrentContextUri = notPlaying.ContextUri;
                CurrentTrackUri = notPlaying.Uri;
                LastPlayOrigin = notPlaying.PlayOrigin;
                break;
            case PausedEvent paused:
                IsPlaying = false;
                CurrentContextUri = paused.ContextUri;
                CurrentTrackUri = paused.Uri;
                LastPlayOrigin = paused.PlayOrigin;
                break;
            case StoppedEvent stopped:
                IsPlaying = false;
                LastPlayOrigin = stopped.PlayOrigin;
                break;
            case SeekEvent seek:
                CurrentPositionMs = seek.Position;
                CurrentDurationMs = seek.Duration;
                CurrentContextUri = seek.ContextUri;
                CurrentTrackUri = seek.Uri;
                LastPlayOrigin = seek.PlayOrigin;
                break;
            case VolumeEvent volume:
                Volume = volume.Value;
                MaxVolume = volume.Max;
                break;
            case ShuffleContextEvent shuffle:
                ShuffleContext = shuffle.Value;
                break;
            case RepeatContextEvent repeatContext:
                RepeatContext = repeatContext.Value;
                break;
            case RepeatTrackEvent repeatTrack:
                RepeatTrack = repeatTrack.Value;
                break;
            case UnknownLibrespotEvent:
                break;
        }

        _recentEvents.Enqueue(@event);
        while (_recentEvents.Count > MaxStoredEvents)
        {
            _recentEvents.TryDequeue(out _);
        }
    }

    public IReadOnlyCollection<LibrespotEvent> GetRecentEvents()
    {
        return _recentEvents.ToArray();
    }
}
