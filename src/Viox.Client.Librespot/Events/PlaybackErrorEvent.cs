namespace Viox.Client.Librespot.Events;

public record PlaybackErrorEvent(
    string ContextUri,
    string Uri,
    string PlayOrigin,
    string Stage,
    string Kind,
    bool Unplayable,
    string Message
) : LibrespotEvent("playback_error");
