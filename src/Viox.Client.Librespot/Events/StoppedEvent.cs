using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Events;

/// <summary>
/// Event emitted when the playback context is empty and player stops.
/// </summary>
public sealed record StoppedEvent : LibrespotEvent
{
    public StoppedEvent() => EventType = "stopped";

    [JsonPropertyName("play_origin")]
    public string PlayOrigin { get; init; } = string.Empty;
}
