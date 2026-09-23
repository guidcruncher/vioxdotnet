using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Models;

/// <summary>
/// Payload to change audio playback volume.
/// </summary>
public sealed record ApiSetVolumeRequest
{
    [JsonPropertyName("volume")]
    public required int Volume { get; init; }

    [JsonPropertyName("relative")]
    public bool Relative { get; init; }
}
