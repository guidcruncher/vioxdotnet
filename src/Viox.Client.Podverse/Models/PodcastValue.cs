using System.Text.Json.Serialization;

namespace Viox.Client.Podverse.Models;

/// <summary>
/// Represents Value4Value payment information associated with a podcast.
/// </summary>
public sealed class PodcastValue
{
    /// <summary>
    /// Gets or sets the payment technology type (e.g., lightning).
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the payment method (e.g., keysend).
    /// </summary>
    [JsonPropertyName("method")]
    public string? Method { get; set; }

    /// <summary>
    /// Gets or sets the suggested payment amount. Uses object to handle both numeric and string values in JSON.
    /// </summary>
    [JsonPropertyName("suggested")]
    public object? Suggested { get; set; }

    /// <summary>
    /// Gets or sets the list of value recipients.
    /// </summary>
    [JsonPropertyName("recipients")]
    public List<ValueRecipient>? Recipients { get; set; }
}
