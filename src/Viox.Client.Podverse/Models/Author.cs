using System.Text.Json.Serialization;

namespace Viox.Client.Podverse.Models;

/// <summary>
/// Represents an author associated with a podcast.
/// </summary>
public sealed class Author
{
    /// <summary>
    /// Gets or sets the unique identifier of the author.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author's display name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the author record is public.
    /// </summary>
    [JsonPropertyName("isPublic")]
    public bool? IsPublic { get; set; }
}
