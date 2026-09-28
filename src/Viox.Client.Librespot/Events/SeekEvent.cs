using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Events;

/// <summary>
/// Event emitted when a seek operation occurs within the track.
/// </summary>
public sealed record SeekEvent : LibrespotEvent
{
    public SeekEvent() => EventType = "seek";

    [JsonPropertyName("context_uri")]
    public string ContextUri { get; init; } = string.Empty;

    [JsonPropertyName("uri")]
    public string Uri { get; init; } = string.Empty;

    [JsonPropertyName("position")]
    public long Position { get; init; }

    [JsonPropertyName("duration")]
    public long Duration { get; init; }

    [JsonPropertyName("play_origin")]
    public string PlayOrigin { get; init; } = string.Empty;
}
