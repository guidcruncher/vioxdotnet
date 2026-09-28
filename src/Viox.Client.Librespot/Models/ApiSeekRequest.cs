using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Models;

/// <summary>
/// Payload to request position seek within active track.
/// </summary>
public sealed record ApiSeekRequest
{
    [JsonPropertyName("position")]
    public required long Position { get; init; }

    [JsonPropertyName("relative")]
    public bool Relative { get; init; }
}
