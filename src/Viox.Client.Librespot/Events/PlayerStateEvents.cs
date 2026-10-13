namespace Viox.Client.Librespot.Events;

public record ActiveEvent() : LibrespotEvent("active");
public record InactiveEvent() : LibrespotEvent("inactive");
