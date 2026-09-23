using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Models;

/// <summary>
/// Payload to update player device name.
/// </summary>
public sealed record ApiSetDeviceNameRequest
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
}
