using System.Text.Json.Serialization;

namespace Viox.Client.Podverse.Models;

/// <summary>
/// Represents a category associated with a podcast.
/// </summary>
public sealed class Category
{
    /// <summary>
    /// Gets or sets the unique identifier of the category.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the label or name of the category.
    /// </summary>
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the category record is public.
    /// </summary>
    [JsonPropertyName("isPublic")]
    public bool? IsPublic { get; set; }
}
