using System.Text.Json.Serialization;

namespace Viox.Client.Podverse.Models;

/// <summary>
/// Represents funding or donation information associated with a podcast.
/// </summary>
public sealed class Funding
{
    /// <summary>
    /// Gets or sets the display text or description for the funding link.
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    /// <summary>
    /// Gets or sets the destination URL for funding/donations.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
