using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Events;

/// <summary>
/// Event emitted when player volume changes.
/// </summary>
public sealed record VolumeEvent : LibrespotEvent
{
    public VolumeEvent() => EventType = "volume";

    [JsonPropertyName("value")]
    public uint Value { get; init; }

    [JsonPropertyName("max")]
    public uint Max { get; init; }
}
