using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Models;

/// <summary>
/// Payload to initiate playback on go-librespot.
/// </summary>
public sealed record ApiPlayRequest
{
    [JsonPropertyName("uri")]
    public required string Uri { get; init; }

    [JsonPropertyName("skip_to_uri")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SkipToUri { get; init; }

    [JsonPropertyName("paused")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Paused { get; init; }

    [JsonPropertyName("position")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? Position { get; init; }
}
