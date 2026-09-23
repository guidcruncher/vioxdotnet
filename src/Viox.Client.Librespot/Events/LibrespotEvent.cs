namespace Viox.Client.Librespot.Events;

/// <summary>
/// Abstract base record for all go-librespot WebSocket events.
/// </summary>
public abstract record LibrespotEvent
{
    public string EventType { get; init; } = string.Empty;
}
