using Viox.Core.Models;

namespace Viox.Client.Mpd.Events;

public interface IMpdEventService
{
    event EventHandler<MpdEvent>? OnEventReceived;
    event EventHandler<MpdTrackStartedEvent>? OnTrackStartedReceived;
    event EventHandler<MpdTrackFinishedEvent>? OnTrackFinishedReceived;
    event EventHandler<MpdPlaybackStartedEvent>? OnPlaybackStartedReceived;
    event EventHandler<MpdPlaybackPausedEvent>? OnPlaybackPausedReceived;
    event EventHandler<MpdSeekedEvent>? OnSeekedReceived;
    event EventHandler<MpdVolumeEvent>? OnVolumeReceived;
    event EventHandler<MpdShuffleChangedEvent>? OnShuffleChangedReceived;
    event EventHandler<MpdRepeatChangedEvent>? OnRepeatChangedReceived;
    event EventHandler<PlaybackState>? OnPlaybackStatusUpdated;
}
