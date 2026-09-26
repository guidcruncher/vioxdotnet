// File: M3uPlaylistParserOptions.cs
namespace Viox.Core.Configuration;

/// <summary>
/// Configuration options for the M3U playlist parser.
/// </summary>
public class M3uPlaylistParserOptions
{
    public const string SectionName = "M3uPlaylist";
    /// <summary>
    /// Gets or sets the default HttpClient name if utilizing IHttpClientFactory.
    /// </summary>
    public string HttpClientName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether non-absolute URIs should be skipped or parsed.
    /// </summary>
    public bool RequireAbsoluteUri { get; set; } = false;
}
