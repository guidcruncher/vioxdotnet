using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Events;

/// <summary>
/// Event emitted when the player is about to play a specified track.
/// </summary>
public sealed record WillPlayEvent : LibrespotEvent
{
    public WillPlayEvent() => EventType = "will_play";

    [JsonPropertyName("context_uri")]
    public string ContextUri { get; init; } = string.Empty;

    [JsonPropertyName("uri")]
    public string Uri { get; init; } = string.Empty;

    [JsonPropertyName("play_origin")]
    public string PlayOrigin { get; init; } = string.Empty;
}
