using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Events;

/// <summary>
/// Event emitted when context repeating setting changes.
/// </summary>
public sealed record RepeatContextEvent : LibrespotEvent
{
    public RepeatContextEvent() => EventType = "repeat_context";

    [JsonPropertyName("value")]
    public bool Value { get; init; }
}
