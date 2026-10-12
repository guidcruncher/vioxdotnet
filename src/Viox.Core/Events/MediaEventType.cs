namespace Viox.Core.Events;

public enum MediaEventType
{
    PlaybackStarted,
    PlaybackPaused,
    PlaybackStopped,

    TrackStarted,
    TrackChanged,
    TrackFinished,

    Seeked,

    VolumeChanged,

    QueueChanged,

    ShuffleChanged,
    RepeatChanged,

    Status
}

