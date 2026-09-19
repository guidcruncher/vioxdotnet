namespace Viox.Core.Configuration;

/// <summary>
/// Configuration options for HTML string processing operations.
/// </summary>
public class HtmlSanitizerOptions
{
    /// <summary>
    /// The configuration section key.
    /// </summary>
    public const string SectionName = "HtmlSanitizer";

    /// <summary>
    /// Gets or sets a value indicating whether whitespace between tags should be trimmed.
    /// </summary>
    public bool TrimWhitespace { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether HTML entities like &amp;amp; or &amp;lt; should be decoded.
    /// </summary>
    public bool DecodeHtmlEntities { get; set; } = true;
}
