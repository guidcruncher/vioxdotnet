using Viox.Core.Models;

namespace Viox.Client.Mpd.Events;

public sealed class MpdEventService : IMpdEventService
{
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

    public void PublishEvent<T>(object sender, T mpdEvent) where T : MpdEvent
    {
        OnEventReceived?.Invoke(sender, mpdEvent);
    }

    public void PublishTrackStarted(object sender, MpdTrackStartedEvent e)
    {
        PublishEvent(sender, e);
        OnTrackStartedReceived?.Invoke(sender, e);
    }

    public void PublishTrackFinished(object sender, MpdTrackFinishedEvent e)
    {
        PublishEvent(sender, e);
        OnTrackFinishedReceived?.Invoke(sender, e);
    }

    public void PublishPlaybackStarted(object sender, MpdPlaybackStartedEvent e)
    {
        PublishEvent(sender, e);
        OnPlaybackStartedReceived?.Invoke(sender, e);
    }

    public void PublishPlaybackPaused(object sender, MpdPlaybackPausedEvent e)
    {
        PublishEvent(sender, e);
        OnPlaybackPausedReceived?.Invoke(sender, e);
    }

    public void PublishSeeked(object sender, MpdSeekedEvent e)
    {
        PublishEvent(sender, e);
        OnSeekedReceived?.Invoke(sender, e);
    }

    public void PublishVolume(object sender, MpdVolumeEvent e)
    {
        PublishEvent(sender, e);
        OnVolumeReceived?.Invoke(sender, e);
    }

    public void PublishShuffleChanged(object sender, MpdShuffleChangedEvent e)
    {
        PublishEvent(sender, e);
        OnShuffleChangedReceived?.Invoke(sender, e);
    }

    public void PublishRepeatChanged(object sender, MpdRepeatChangedEvent e)
    {
        PublishEvent(sender, e);
        OnRepeatChangedReceived?.Invoke(sender, e);
    }

    public void PublishStatus(object sender, PlaybackState state)
    {
        OnPlaybackStatusUpdated?.Invoke(sender, state);
    }
}
