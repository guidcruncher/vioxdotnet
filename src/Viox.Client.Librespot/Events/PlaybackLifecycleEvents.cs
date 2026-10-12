namespace Viox.Client.Librespot.Events;

public record WillPlayEvent(string ContextUri, string Uri, string PlayOrigin) : LibrespotEvent("will_play");

public record PlayingEvent(string ContextUri, string Uri, bool Resume, string PlayOrigin) : LibrespotEvent("playing");

public record NotPlayingEvent(string ContextUri, string Uri, string PlayOrigin) : LibrespotEvent("not_playing");

public record PausedEvent(string ContextUri, string Uri, string PlayOrigin) : LibrespotEvent("paused");

public record StoppedEvent(string PlayOrigin) : LibrespotEvent("stopped");
