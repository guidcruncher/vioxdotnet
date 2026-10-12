namespace Viox.Client.Librespot.Events;

public record VolumeEvent(int Value, int Max) : LibrespotEvent("volume");
