namespace Viox.Client.Librespot.Events;

public interface ILibrespotEventService
{
    event EventHandler<LibrespotEvent>? OnEventReceived;
    event EventHandler<ActiveEvent>? OnActiveReceived;
    event EventHandler<InactiveEvent>? OnInactiveReceived;
    event EventHandler<MetadataEvent>? OnMetadataReceived;
    event EventHandler<WillPlayEvent>? OnWillPlayReceived;
    event EventHandler<PlayingEvent>? OnPlayingReceived;
    event EventHandler<NotPlayingEvent>? OnNotPlayingReceived;
    event EventHandler<PausedEvent>? OnPausedReceived;
    event EventHandler<StoppedEvent>? OnStoppedReceived;
    event EventHandler<PlaybackErrorEvent>? OnPlaybackErrorReceived;
    event EventHandler<SeekEvent>? OnSeekReceived;
    event EventHandler<VolumeEvent>? OnVolumeReceived;
    event EventHandler<ShuffleContextEvent>? OnShuffleContextReceived;
    event EventHandler<RepeatContextEvent>? OnRepeatContextReceived;
    event EventHandler<RepeatTrackEvent>? OnRepeatTrackReceived;

    void DispatchEvent(string rawJson);
}
