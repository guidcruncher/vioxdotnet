using System.Text.Json.Serialization;

namespace Viox.Client.Podverse.Models;

/// <summary>
/// Represents a user account object returned by the Podverse API.
/// </summary>
public sealed class User
{
    /// <summary>
    /// Gets or sets the unique user identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's primary email address.
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's display name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the array of subscribed podcast identifiers.
    /// </summary>
    [JsonPropertyName("subscribedPodcastIds")]
    public List<string>? SubscribedPodcastIds { get; set; }
}
