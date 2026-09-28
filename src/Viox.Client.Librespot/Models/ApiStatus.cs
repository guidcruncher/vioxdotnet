using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Models;

/// <summary>
/// Represents the full player state in go-librespot.
/// </summary>
public sealed record ApiStatus
{
    [JsonPropertyName("username")]
    public string Username { get; init; } = string.Empty;

    [JsonPropertyName("device_id")]
    public string DeviceId { get; init; } = string.Empty;

    [JsonPropertyName("device_type")]
    public string DeviceType { get; init; } = string.Empty;

    [JsonPropertyName("device_name")]
    public string DeviceName { get; init; } = string.Empty;

    [JsonPropertyName("play_origin")]
    public string PlayOrigin { get; init; } = string.Empty;

    [JsonPropertyName("stopped")]
    public bool Stopped { get; init; }

    [JsonPropertyName("paused")]
    public bool Paused { get; init; }

    [JsonPropertyName("buffering")]
    public bool Buffering { get; init; }

    [JsonPropertyName("volume")]
    public uint Volume { get; init; }

    [JsonPropertyName("volume_steps")]
    public uint VolumeSteps { get; init; }

    [JsonPropertyName("repeat_context")]
    public bool RepeatContext { get; init; }

    [JsonPropertyName("repeat_track")]
    public bool RepeatTrack { get; init; }

    [JsonPropertyName("shuffle_context")]
    public bool ShuffleContext { get; init; }

    [JsonPropertyName("track")]
    public ApiTrack? Track { get; init; }

    /// <summary>
    /// Gets a value indicating whether a track is currently actively playing.
    /// </summary>
    [JsonIgnore]
    [JsonPropertyName("is_playing")]
    public bool IsPlaying => !Stopped && !Paused && Track is not null;
}
