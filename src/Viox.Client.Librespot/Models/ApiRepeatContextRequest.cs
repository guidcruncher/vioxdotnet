using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Models;

/// <summary>
/// Payload to toggle repeat context mode.
/// </summary>
public sealed record ApiRepeatContextRequest
{
    [JsonPropertyName("repeat_context")]
    public required bool RepeatContext { get; init; }
}
