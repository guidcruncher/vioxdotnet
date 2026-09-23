namespace Viox.Core.Configuration;

/// <summary>
/// Configuration options for podcast episode parsing operations.
/// </summary>
public class PodcastEpisodeParserOptions
{
    public const string SectionName = "PodcastEpisodeParser";

    /// <summary>
    /// Gets or sets a value indicating whether iTunes summary should be used as a fallback if description is absent.
    /// </summary>
    public bool FallbackToItunesSummary { get; set; } = true;
}
