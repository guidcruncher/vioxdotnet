namespace Viox.Core.Configuration;

/// <summary>
/// Configurable options for the current media singleton service.
/// </summary>
public sealed class CurrentMediaServiceOptions
{

    public const string SectionName = "CurrentMedia";

    /// <summary>
    /// Gets or sets a value indicating whether changes to current media should be logged automatically.
    /// </summary>
    public bool EnableStateChangeLogging { get; set; } = true;
}
