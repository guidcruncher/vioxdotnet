using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Models;

/// <summary>
/// Payload to toggle shuffle context mode.
/// </summary>
public sealed record ApiShuffleContextRequest
{
    [JsonPropertyName("shuffle_context")]
    public required bool ShuffleContext { get; init; }
}
