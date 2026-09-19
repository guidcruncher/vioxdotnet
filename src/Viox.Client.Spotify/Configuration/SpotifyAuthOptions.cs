namespace Viox.Client.Spotify.Configuration;

/// <summary>
/// Configuration options for Spotify Application credentials and settings.
/// </summary>
public sealed class SpotifyAuthOptions
{
    /// <summary>
    /// The configuration section name within appsettings.json.
    /// </summary>
    public const string SectionName = "Spotify";

    /// <summary>
    /// Gets or sets the Spotify Application Client ID.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Spotify Application Client Secret.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Redirect URI configured in the Spotify Developer Dashboard.
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets scopes requested for user authorization.
    /// </summary>
    public List<string> Scopes { get; set; } = new();
}
