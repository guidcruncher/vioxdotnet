using System.Text.Json.Serialization;

namespace Viox.Client.Podverse.Models;

/// <summary>
/// Represents an RSS feed URL object associated with a podcast.
/// </summary>
public sealed class FeedUrl
{
    /// <summary>
    /// Gets or sets the unique identifier of the feed URL record, if present.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the RSS feed web address.
    /// </summary>
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the feed URL record is public.
    /// </summary>
    [JsonPropertyName("isPublic")]
    public bool? IsPublic { get; set; }
}
