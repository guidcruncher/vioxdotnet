using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Events;

/// <summary>
/// Event emitted when context shuffling setting changes.
/// </summary>
public sealed record ShuffleContextEvent : LibrespotEvent
{
    public ShuffleContextEvent() => EventType = "shuffle_context";

    [JsonPropertyName("value")]
    public bool Value { get; init; }
}
