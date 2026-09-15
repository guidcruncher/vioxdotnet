namespace Viox.Client.RadioBrowser.Configuration;

/// <summary>
/// Configuration for <see cref="RadioBrowserClient"/>.
/// </summary>
public sealed class RadioBrowserOptions
{
    /// <summary>
    /// Default user agent used when the caller does not supply one.
    /// Radio Browser requires a descriptive product token rather than a generic runtime UA.
    /// </summary>
    public const string DefaultUserAgent = "Viox.Client.RadioBrowser/1.0 (+https://www.radio-browser.info/)";

    /// <summary>
    /// Gets or sets the API origin, for example <c>https://de1.api.radio-browser.info</c>.
    /// When <see langword="null"/> the client uses <see cref="FallbackBaseAddress"/>.
    /// </summary>
    public Uri? BaseAddress { get; set; }

    /// <summary>
    /// Gets or sets the address used when <see cref="BaseAddress"/> is not set.
    /// Defaults to the well-known German mirror.
    /// </summary>
    public Uri FallbackBaseAddress { get; set; } = new("https://de1.api.radio-browser.info");

    /// <summary>
    /// Gets or sets the <c>User-Agent</c> sent on every request.
    /// Radio Browser uses this for usage statistics and may throttle empty or generic agents.
    /// </summary>
    public string UserAgent { get; set; } = DefaultUserAgent;

    /// <summary>
    /// Gets or sets an optional HTTP timeout applied when the client creates its own <see cref="HttpClient"/>.
    /// Ignored when an external <see cref="HttpClient"/> is supplied.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets whether JSON property names should be read case-insensitively.
    /// </summary>
    public bool PropertyNameCaseInsensitive { get; set; } = true;
}
