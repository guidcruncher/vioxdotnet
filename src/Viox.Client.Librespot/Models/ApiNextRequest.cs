using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Models;

/// <summary>
/// Payload to skip to next track.
/// </summary>
public sealed record ApiNextRequest
{
    [JsonPropertyName("uri")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Uri { get; init; }
}
