namespace Viox.Core.Services;

/// <summary>
/// Provides methods for stripping HTML tags and decoding HTML entities from text strings.
/// </summary>
public interface IHtmlSanitizerService
{
    /// <summary>
    /// Removes all HTML tags from the provided string and returns plain text.
    /// </summary>
    /// <param name="htmlInput">The raw string containing HTML content.</param>
    /// <returns>A string free of HTML tags with decoded text entities.</returns>
    string StripHtml(string? htmlInput);
}
