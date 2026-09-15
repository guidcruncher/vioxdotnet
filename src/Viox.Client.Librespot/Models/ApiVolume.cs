using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Models;

/// <summary>
/// Represents current volume settings.
/// </summary>
public sealed record ApiVolume
{
    [JsonPropertyName("value")]
    public uint Value { get; init; }

    [JsonPropertyName("max")]
    public uint Max { get; init; }
}
