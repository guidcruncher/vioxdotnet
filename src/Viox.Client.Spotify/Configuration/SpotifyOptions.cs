namespace Viox.Client.Spotify.Configuration;

/// <summary>
/// Configuration options for the Spotify API Client.
/// </summary>
public sealed class SpotifyOptions
{
    /// <summary>
    /// Section name used in configuration files.
    /// </summary>
    public const string SectionName = "Spotify";

    /// <summary>
    /// Base address of the Spotify Web API. Defaults to https://api.spotify.com/v1.
    /// </summary>
    public Uri BaseAddress { get; set; } = new Uri("https://api.spotify.com/v1/", UriKind.Absolute);

    /// <summary>
    /// Bearer access token for authorization header injection.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Request timeout limit for Spotify requests. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
