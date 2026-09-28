using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Events;

/// <summary>
/// Event emitted when track repeating setting changes.
/// </summary>
public sealed record RepeatTrackEvent : LibrespotEvent
{
    public RepeatTrackEvent() => EventType = "repeat_track";

    [JsonPropertyName("value")]
    public bool Value { get; init; }
}
