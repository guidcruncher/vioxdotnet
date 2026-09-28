namespace Viox.Core.Configuration;

/// <summary>
/// Configuration options for <see cref="Viox.Core.Services.MediaSearchService"/>.
/// </summary>
public class MediaSearchOptions
{
    public const string SectionName = "MediaSearch";

    /// <summary>
    /// Gets or sets the default search timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum page size allowed per source query.
    /// </summary>
    public int MaxPageLimit { get; set; } = 100;
}
