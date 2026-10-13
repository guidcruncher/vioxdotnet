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

    void PublishEvent<T>(object sender, T mpdEvent) where T : MpdEvent;
    void PublishTrackStarted(object sender, MpdTrackStartedEvent e);
    void PublishTrackFinished(object sender, MpdTrackFinishedEvent e);
    void PublishPlaybackStarted(object sender, MpdPlaybackStartedEvent e);
    void PublishPlaybackPaused(object sender, MpdPlaybackPausedEvent e);
    void PublishSeeked(object sender, MpdSeekedEvent e);
    void PublishVolume(object sender, MpdVolumeEvent e);
    void PublishShuffleChanged(object sender, MpdShuffleChangedEvent e);
    void PublishRepeatChanged(object sender, MpdRepeatChangedEvent e);
    void PublishStatus(object sender, PlaybackState state);
}

