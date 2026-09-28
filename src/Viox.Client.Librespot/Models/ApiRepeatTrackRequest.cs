using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Models;

/// <summary>
/// Payload to toggle repeat track mode.
/// </summary>
public sealed record ApiRepeatTrackRequest
{
    [JsonPropertyName("repeat_track")]
    public required bool RepeatTrack { get; init; }
}
