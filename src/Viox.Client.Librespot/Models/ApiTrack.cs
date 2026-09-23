using System.Text.Json.Serialization;

namespace Viox.Client.Librespot.Models;

/// <summary>
/// Represents a Spotify track or podcast episode.
/// </summary>
public sealed record ApiTrack
{
    [JsonPropertyName("uri")]
    public string Uri { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("artist_names")]
    public IReadOnlyList<string> ArtistNames { get; init; } = [];

    [JsonPropertyName("album_name")]
    public string AlbumName { get; init; } = string.Empty;

    [JsonPropertyName("album_cover_url")]
    public string? AlbumCoverUrl { get; init; }

    [JsonPropertyName("position")]
    public long Position { get; init; }

    [JsonPropertyName("duration")]
    public int Duration { get; init; }

    [JsonPropertyName("release_date")]
    public string ReleaseDate { get; init; } = string.Empty;

    [JsonPropertyName("track_number")]
    public int TrackNumber { get; init; }

    [JsonPropertyName("disc_number")]
    public int DiscNumber { get; init; }

    [JsonPropertyName("format")]
    public string Format { get; init; } = string.Empty;

    [JsonPropertyName("codec")]
    public string Codec { get; init; } = string.Empty;

    [JsonPropertyName("bitrate")]
    public int? Bitrate { get; init; }

    [JsonPropertyName("sample_rate")]
    public int? SampleRate { get; init; }

    [JsonPropertyName("bit_depth")]
    public int? BitDepth { get; init; }
}
