using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Events;

/// <summary>
/// Event emitted when the track starts or resumes playing.
/// </summary>
public sealed record PlayingEvent : LibrespotEvent
{
    public PlayingEvent() => EventType = "playing";

    [JsonPropertyName("context_uri")]
    public string ContextUri { get; init; } = string.Empty;

    [JsonPropertyName("uri")]
    public string Uri { get; init; } = string.Empty;

    [JsonPropertyName("resume")]
    public bool Resume { get; init; }

    [JsonPropertyName("play_origin")]
    public string PlayOrigin { get; init; } = string.Empty;
}
