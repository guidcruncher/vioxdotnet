using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Client.Spotify.Configuration;
using Viox.Client.Spotify.Models;

namespace Viox.Client.Spotify.Services;

/// <summary>
/// Service performing Spotify authorization code flow operations using HttpClient.
/// </summary>
public sealed class SpotifyAuthService : ISpotifyAuthService
{
    private readonly HttpClient _httpClient;
    private readonly SpotifyAuthOptions _options;
    private readonly ILogger<SpotifyAuthService> _logger;
    private readonly IAuthTokenStore _tokenStore;

    /// <summary>
    /// Initializes a new instance of <see cref="SpotifyAuthService"/>.
    /// </summary>
    /// <param name="httpClient">HttpClient instance created by IHttpClientFactory.</param>
    /// <param name="tokenStore">Token store.</param>
    /// <param name="options">Configured Spotify options.</param>
    /// <param name="logger">Standard logging framework logger.</param>
    public SpotifyAuthService(
        HttpClient httpClient,
        IAuthTokenStore tokenStore,
        IOptions<SpotifyAuthOptions> options,
        ILogger<SpotifyAuthService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string BuildAuthorizationUrl(string state)
    {
        var queryParams = new Dictionary<string, string>
        {
            ["response_type"] = "code",
            ["client_id"] = _options.ClientId,
            ["scope"] = string.Join(" ", _options.Scopes),
            ["redirect_uri"] = _options.RedirectUri,
            ["state"] = state
        };

        var queryString = string.Join("&", queryParams.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        return $"https://accounts.spotify.com/authorize?{queryString}";
    }

    /// <inheritdoc />
    public async Task<SpotifyTokenResponse?> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken = default)
    {
        var formBody = new Dictionary<string, string>
        {
            ["code"] = code,
            ["redirect_uri"] = _options.RedirectUri,
            ["grant_type"] = "authorization_code"
        };

        return await RequestTokenAsync(formBody, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SpotifyTokenResponse?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var formBody = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken
        };

        return await RequestTokenAsync(formBody, cancellationToken);
    }

    private async Task<SpotifyTokenResponse?> RequestTokenAsync(Dictionary<string, string> formBody, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");

            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            request.Content = new FormUrlEncodedContent(formBody);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Spotify Token request failed. Status: {StatusCode}, Response: {Content}", response.StatusCode, content);
                return null;
            }

            var tokenResponse = JsonSerializer.Deserialize<SpotifyTokenResponse>(content);
            if (tokenResponse is null)
            {
                _logger.LogError("Failed to deserialize Spotify token response payload. Content: {Content}", content);
                return null;
            }

            await _tokenStore.SaveTokenAsync(TokenData.FromSpotifyTokenResponse(tokenResponse), cancellationToken);

            return tokenResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while communicating with Spotify Auth API.");
            throw;
        }
    }

    public async Task<string> GetAccessToken(CancellationToken ct = default)
    {
        TokenData? token = await _tokenStore.GetTokenAsync(ct);
        if (token is null || string.IsNullOrEmpty(token.AccessToken))
        {
            return string.Empty;
        }

        return token.AccessToken;
    }

    public async Task<string> GetRefreshToken(CancellationToken ct = default)
    {
        TokenData? token = await _tokenStore.GetTokenAsync(ct);
        if (token is null || string.IsNullOrEmpty(token.RefreshToken))
        {
            return string.Empty;
        }
        return token.RefreshToken;
    }

}

