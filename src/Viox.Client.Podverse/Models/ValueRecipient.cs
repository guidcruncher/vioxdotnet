using System.Text.Json.Serialization;

namespace Viox.Client.Podverse.Models;

/// <summary>
/// Represents a recipient of Value4Value payments.
/// </summary>
public sealed class ValueRecipient
{
    /// <summary>
    /// Gets or sets the name or description of the payment recipient.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the recipient type (e.g., node, lnaddress).
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the node address or lightning address.
    /// </summary>
    [JsonPropertyName("address")]
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the revenue split percentage or share for this recipient.
    /// </summary>
    [JsonPropertyName("split")]
    public double? Split { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this recipient processes fee payments.
    /// </summary>
    [JsonPropertyName("fee")]
    public bool? Fee { get; set; }

    /// <summary>
    /// Gets or sets an optional custom key for keysend payments.
    /// </summary>
    [JsonPropertyName("customKey")]
    public string? CustomKey { get; set; }

    /// <summary>
    /// Gets or sets an optional custom value payload for keysend payments.
    /// </summary>
    [JsonPropertyName("customValue")]
    public string? CustomValue { get; set; }
}
