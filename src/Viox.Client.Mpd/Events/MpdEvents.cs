using Viox.Core.Models;

namespace Viox.Client.Mpd.Events;

public record MpdEvent(string Type);

public record MpdTrackStartedEvent(MediaMetaData? Track) : MpdEvent("track_started");

public record MpdTrackFinishedEvent(MediaMetaData? Track) : MpdEvent("track_finished");

public record MpdPlaybackStartedEvent(double Position, bool Playing) : MpdEvent("playback_started");

public record MpdPlaybackPausedEvent(double Position) : MpdEvent("playback_paused");

public record MpdSeekedEvent(double Position, double Duration) : MpdEvent("seeked");

public record MpdVolumeEvent(int Value, int Max) : MpdEvent("volume");

public record MpdShuffleChangedEvent(bool Value) : MpdEvent("shuffle_changed");

public record MpdRepeatChangedEvent(bool Value) : MpdEvent("repeat_changed");

