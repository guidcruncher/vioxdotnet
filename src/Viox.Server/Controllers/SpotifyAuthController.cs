using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Viox.Client.Spotify.Services;

namespace Viox.Server.Controllers;

/// <summary>
/// Controller for managing Spotify authorization code flow logic.
/// </summary>
[ApiController]
[Route("/api/v1/auth")]
[Tags("Spotify Auth")]
[Produces("application/json")]
public sealed class SpotifyAuthController : ControllerBase
{
    private const string StateCookieName = "spotify_auth_state";
    private readonly ISpotifyAuthService _spotifyAuthService;
    private readonly ILogger<SpotifyAuthController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="SpotifyAuthController"/>.
    /// </summary>
    /// <param name="spotifyAuthService">Service for Spotify OAuth processing.</param>
    /// <param name="logger">Logger instance.</param>
    public SpotifyAuthController(
        ISpotifyAuthService spotifyAuthService,
        ILogger<SpotifyAuthController> logger)
    {
        _spotifyAuthService = spotifyAuthService ?? throw new ArgumentNullException(nameof(spotifyAuthService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initiates the Spotify authorization process by generating a state token and redirecting to Spotify.
    /// </summary>
    /// <returns>Redirect result to Spotify consent screen.</returns>
    [HttpGet("login")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult Login()
    {
        var state = Guid.NewGuid().ToString("N");

        Response.Cookies.Append(StateCookieName, state, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(10)
        });

        var authUrl = _spotifyAuthService.BuildAuthorizationUrl(state);
        _logger.LogInformation("Redirecting user to Spotify Auth URL.");
        return Redirect(authUrl);
    }

    /// <summary>
    /// Handles callback redirect from Spotify after user completes consent flow.
    /// </summary>
    /// <param name="code">Authorization code returned by Spotify.</param>
    /// <param name="state">State parameter returned by Spotify for CSRF verification.</param>
    /// <param name="error">Error parameter returned if authorization was denied.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON result containing access token details.</returns>
    [HttpGet("callback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Callback(
        [FromQuery] string? code,
        [FromQuery] string? state,
        [FromQuery] string? error,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogWarning("Spotify authorization failed with error: {Error}", error);
            return BadRequest(new { Error = error });
        }

        if (!Request.Cookies.TryGetValue(StateCookieName, out var savedState) || savedState != state)
        {
            _logger.LogWarning("State validation failed. State mismatch or missing cookie.");
            return Unauthorized("Invalid state parameter. Authorization request rejected for security.");
        }

        Response.Cookies.Delete(StateCookieName);

        if (string.IsNullOrEmpty(code))
        {
            return BadRequest("Authorization code is missing.");
        }

        var tokenResponse = await _spotifyAuthService.ExchangeCodeForTokenAsync(code, cancellationToken);
        if (tokenResponse is null)
        {
            return BadRequest("Failed to retrieve access token from Spotify.");
        }

        return Ok(tokenResponse);
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// </summary>
    /// <param name="refreshToken">The Spotify refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Fresh access token details.</returns>
    [HttpPost("refresh")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] string refreshToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return BadRequest("Refresh token is required.");
        }

        var tokenResponse = await _spotifyAuthService.RefreshTokenAsync(refreshToken, cancellationToken);
        if (tokenResponse is null)
        {
            return BadRequest("Failed to refresh access token.");
        }

        return Ok(tokenResponse);
    }
}
