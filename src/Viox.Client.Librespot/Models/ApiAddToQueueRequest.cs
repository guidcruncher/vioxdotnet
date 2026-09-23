using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Models;

/// <summary>
/// Payload to add item to playback queue.
/// </summary>
public sealed record ApiAddToQueueRequest
{
    [JsonPropertyName("uri")]
    public required string Uri { get; init; }
}
