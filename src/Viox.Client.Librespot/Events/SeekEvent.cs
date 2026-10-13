namespace Viox.Client.Librespot.Events;

public record SeekEvent(
    string ContextUri,
    string Uri,
    long Position,
    long Duration,
    string PlayOrigin
) : LibrespotEvent("seek");
