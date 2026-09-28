using System.Text.Json.Serialization;

namespace Viox.Client.Podverse.Models;

/// <summary>
/// Represents an episode model returned by the Podverse API.
/// </summary>
public sealed class Episode
{
    /// <summary>
    /// Gets or sets the unique identifier of the episode.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the episode title.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the episode description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the direct media URL for the episode audio or video.
    /// </summary>
    [JsonPropertyName("mediaUrl")]
    public string? MediaUrl { get; set; }

    /// <summary>
    /// Gets or sets the media duration in seconds.
    /// </summary>
    [JsonPropertyName("duration")]
    public int? Duration { get; set; }

    /// <summary>
    /// Gets or sets the publication date of the episode.
    /// </summary>
    [JsonPropertyName("pubDate")]
    public string? PubDate { get; set; }

    /// <summary>
    /// Gets or sets the parent podcast object associated with this episode.
    /// </summary>
    [JsonPropertyName("podcast")]
    public Podcast? Podcast { get; set; }
}
