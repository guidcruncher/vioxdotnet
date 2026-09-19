namespace Viox.Client.Librespot.Events;

/// <summary>
/// Event emitted when the player device becomes inactive.
/// </summary>
public sealed record InactiveEvent : LibrespotEvent
{
    public InactiveEvent() => EventType = "inactive";
}
