namespace Viox.Client.Librespot.Events;

public record VolumeEvent(int Value, int Max) : LibrespotEvent("volume");

public record ShuffleContextEvent(bool Value) : LibrespotEvent("shuffle_context");

public record RepeatContextEvent(bool Value) : LibrespotEvent("repeat_context");

public record RepeatTrackEvent(bool Value) : LibrespotEvent("repeat_track");
