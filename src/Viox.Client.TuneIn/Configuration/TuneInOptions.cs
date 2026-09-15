namespace Viox.Client.TuneIn.Configuration;

/// <summary>
/// Configuration options for configuring the TuneIn API client.
/// </summary>
public sealed class TuneInOptions
{
    /// <summary>
    /// Configuration section key for binding settings from application configuration.
    /// </summary>
    public const string SectionName = "TuneIn";

    /// <summary>
    /// Base address of the TuneIn OPML API endpoint.
    /// Defaults to <c>https://opml.radiotime.com/</c>.
    /// </summary>
    public Uri BaseAddress { get; set; } = new("https://opml.radiotime.com/");

    /// <summary>
    /// Optional partner identifier used to extend API rate limits.
    /// </summary>
    public string? PartnerId { get; set; }

    /// <summary>
    /// Request timeout for API calls in seconds. Defaults to 30 seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Optional custom User-Agent header value sent with requests.
    /// </summary>
    public string UserAgent { get; set; } = "Viox.Client.TuneIn/1.0";
}
