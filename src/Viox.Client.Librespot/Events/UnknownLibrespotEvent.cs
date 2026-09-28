using System.Text.Json;

namespace Viox.Client.Librespot.Events;

/// <summary>
/// Fallback event record when an unrecognized event type is received.
/// </summary>
public sealed record UnknownLibrespotEvent : LibrespotEvent
{
    public UnknownLibrespotEvent() => EventType = "unknown";

    public JsonElement RawData { get; init; }
}
