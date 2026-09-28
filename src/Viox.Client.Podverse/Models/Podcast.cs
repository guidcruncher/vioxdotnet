using System.Text.Json.Serialization;

namespace Viox.Client.Podverse.Models;

/// <summary>
/// Represents a podcast model returned by the Podverse API.
/// </summary>
public sealed class Podcast
{
    /// <summary>
    /// Gets the Uri
    /// </summary>
    [JsonPropertyName("uri")]
    public string Uri { get => $"podverse:podcast:{Id}"; }

    /// <summary>
    /// Gets or sets the unique identifier of the podcast.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Podcast Index identifier.
    /// </summary>
    [JsonPropertyName("podcastIndexId")]
    public string? PodcastIndexId { get; set; }

    /// <summary>
    /// Gets or sets the Podcast GUID identifier.
    /// </summary>
    [JsonPropertyName("podcastGuid")]
    public string? PodcastGuid { get; set; }

    /// <summary>
    /// Gets or sets the authority identifier.
    /// </summary>
    [JsonPropertyName("authorityId")]
    public string? AuthorityId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether credentials are required.
    /// </summary>
    [JsonPropertyName("credentialsRequired")]
    public bool? CredentialsRequired { get; set; }

    /// <summary>
    /// Gets or sets the podcast title.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the sortable title string.
    /// </summary>
    [JsonPropertyName("sortableTitle")]
    public string? SortableTitle { get; set; }

    /// <summary>
    /// Gets or sets the podcast subtitle.
    /// </summary>
    [JsonPropertyName("subtitle")]
    public string? Subtitle { get; set; }

    /// <summary>
    /// Gets or sets the podcast description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the primary image URL.
    /// </summary>
    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the shrunk image URL.
    /// </summary>
    [JsonPropertyName("shrunkImageUrl")]
    public string? ShrunkImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the podcast website link URL.
    /// </summary>
    [JsonPropertyName("linkUrl")]
    public string? LinkUrl { get; set; }

    /// <summary>
    /// Gets or sets the media format medium (e.g., podcast, video).
    /// </summary>
    [JsonPropertyName("medium")]
    public string? Medium { get; set; }

    /// <summary>
    /// Gets or sets the iTunes feed type string (e.g., episodic, serial).
    /// </summary>
    [JsonPropertyName("itunesFeedType")]
    public string? ItunesFeedType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the podcast contains explicit content.
    /// </summary>
    [JsonPropertyName("isExplicit")]
    public bool? IsExplicit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the podcast is publicly visible.
    /// </summary>
    [JsonPropertyName("isPublic")]
    public bool? IsPublic { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the podcast has live items.
    /// </summary>
    [JsonPropertyName("hasLiveItem")]
    public bool? HasLiveItem { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the podcast has seasons.
    /// </summary>
    [JsonPropertyName("hasSeasons")]
    public bool? HasSeasons { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the podcast contains video.
    /// </summary>
    [JsonPropertyName("hasVideo")]
    public bool? HasVideo { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether dynamic ads warnings are hidden.
    /// </summary>
    [JsonPropertyName("hideDynamicAdsWarning")]
    public bool? HideDynamicAdsWarning { get; set; }

    /// <summary>
    /// Gets or sets the canonical language code for the podcast.
    /// </summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the feed was last updated.
    /// </summary>
    [JsonPropertyName("feedLastUpdated")]
    public string? FeedLastUpdated { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the podcast was last found in Podcast Index.
    /// </summary>
    [JsonPropertyName("lastFoundInPodcastIndex")]
    public string? LastFoundInPodcastIndex { get; set; }

    /// <summary>
    /// Gets or sets the publication timestamp of the last episode.
    /// </summary>
    [JsonPropertyName("lastEpisodePubDate")]
    public string? LastEpisodePubDate { get; set; }

    /// <summary>
    /// Gets or sets the title of the last episode.
    /// </summary>
    [JsonPropertyName("lastEpisodeTitle")]
    public string? LastEpisodeTitle { get; set; }

    /// <summary>
    /// Gets or sets the latest live item status string.
    /// </summary>
    [JsonPropertyName("latestLiveItemStatus")]
    public string? LatestLiveItemStatus { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp of the record.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets funding sources associated with this podcast.
    /// </summary>
    [JsonPropertyName("funding")]
    public List<Funding>? Funding { get; set; }

    /// <summary>
    /// Gets or sets Value4Value details for this podcast.
    /// </summary>
    [JsonPropertyName("value")]
    public List<PodcastValue>? Value { get; set; }

    /// <summary>
    /// Gets or sets feed URLs associated with this podcast.
    /// </summary>
    [JsonPropertyName("feedUrls")]
    public List<FeedUrl>? FeedUrls { get; set; }

    /// <summary>
    /// Gets or sets authors associated with this podcast.
    /// </summary>
    [JsonPropertyName("authors")]
    public List<Author>? Authors { get; set; }

    /// <summary>
    /// Gets or sets categories associated with this podcast.
    /// </summary>
    [JsonPropertyName("categories")]
    public List<Category>? Categories { get; set; }

    /// <summary>
    /// Gets or sets episodes associated with this podcast.
    /// </summary>
    [JsonPropertyName("episodes")]
    public List<Episode>? Episodes { get; set; }
}
