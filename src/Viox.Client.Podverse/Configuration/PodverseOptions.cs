namespace Viox.Client.Podverse.Configuration;

/// <summary>
/// Represents configuration options for the Podverse API client.
/// </summary>
public sealed class PodverseOptions
{
    public const string SectionName = "Podverse";

    /// <summary>
    /// Gets or sets the base URL for the Podverse API endpoint.
    /// Defaults to https://api.podverse.fm/api/v1
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.podverse.fm/api/v1";

    /// <summary>
    /// Gets or sets the authorization token sent in the API request header.
    /// </summary>
    public string? AuthorizationToken { get; set; }

    /// <summary>
    /// Gets or sets the HTTP request timeout duration in seconds. Defaults to 30.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
