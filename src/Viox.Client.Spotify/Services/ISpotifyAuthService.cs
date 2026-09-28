using Viox.Client.Spotify.Models;

namespace Viox.Client.Spotify.Services;

/// <summary>
/// Defines methods for Spotify OAuth 2.0 Authorization Code Flow authentication operations.
/// </summary>
public interface ISpotifyAuthService
{
    /// <summary>
    /// Generates a Spotify authorization URL for directing users to consent page.
    /// </summary>
    /// <param name="state">A state token for CSRF validation.</param>
    /// <returns>The fully formatted authorization URI string.</returns>
    string BuildAuthorizationUrl(string state);

    /// <summary>
    /// Exchanges an authorization code for access and refresh tokens.
    /// </summary>
    /// <param name="code">The authorization code returned by Spotify.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="SpotifyTokenResponse"/> containing tokens.</returns>
    Task<SpotifyTokenResponse?> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A new <see cref="SpotifyTokenResponse"/> containing fresh access tokens.</returns>
    Task<SpotifyTokenResponse?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the current Access token
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A string containing access token.</returns>
    Task<string> GetAccessToken(CancellationToken ct = default);

    /// <summary>
    /// Returns the current Refresh token
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A string containing access token.</returns>
    Task<string> GetRefreshToken(CancellationToken ct = default);
}
