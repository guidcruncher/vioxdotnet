namespace Viox.Core.Events;

public enum MediaEventType
{
    PlaybackStarted,
    PlaybackPaused,
    PlaybackStopped,
    TrackStarted,
    TrackChanged,
    TrackFinished,
    PlaybackError,
    Seeked,
    VolumeChanged,
    QueueChanged,
    ShuffleChanged,
    RepeatChanged,
    DeviceStateChanged,
    Status
}
