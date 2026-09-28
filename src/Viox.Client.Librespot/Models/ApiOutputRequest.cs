using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Models;

/// <summary>
/// Payload to reopen backend audio output device.
/// </summary>
public sealed record ApiOutputRequest
{
    [JsonPropertyName("device")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Device { get; init; }
}
