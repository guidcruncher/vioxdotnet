using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Events;

/// <summary>
/// Event emitted when the current track finishes playing.
/// </summary>
public sealed record NotPlayingEvent : LibrespotEvent
{
    public NotPlayingEvent() => EventType = "not_playing";

    [JsonPropertyName("context_uri")]
    public string ContextUri { get; init; } = string.Empty;

    [JsonPropertyName("uri")]
    public string Uri { get; init; } = string.Empty;

    [JsonPropertyName("play_origin")]
    public string PlayOrigin { get; init; } = string.Empty;
}
