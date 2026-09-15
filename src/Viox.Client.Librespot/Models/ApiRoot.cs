using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Models;

/// <summary>
/// Represents daemon reachability and playback readiness.
/// </summary>
public sealed record ApiRoot
{
    [JsonPropertyName("playback_ready")]
    public bool PlaybackReady { get; init; }
}
