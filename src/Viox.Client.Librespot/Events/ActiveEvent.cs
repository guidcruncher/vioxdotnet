namespace Viox.Client.Librespot.Events;

/// <summary>
/// Event emitted when the player device becomes active.
/// </summary>
public sealed record ActiveEvent : LibrespotEvent
{
    public ActiveEvent() => EventType = "active";
}
