using System.Text.Json.Serialization;

namespace Viox.Client.Spotify.Models;

/// <summary>
/// Represents the JSON payload returned by Spotify's token endpoint.
/// </summary>
public sealed class SpotifyTokenResponse
{
    /// <summary>
    /// Gets or sets the access token used to authenticate calls to Spotify Web API.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of token (always "Bearer").
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of granted scopes.
    /// </summary>
    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the lifetime of the access token in seconds.
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Gets or sets the refresh token used to retrieve new access tokens.
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
}
