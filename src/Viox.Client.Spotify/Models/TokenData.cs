namespace Viox.Client.Spotify.Models;

public sealed class TokenData
{
    public string? Code { get; set; }
    public string? State { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? TokenType { get; set; }
    public DateTimeOffset? ExpiresAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public static TokenData FromSpotifyTokenResponse(SpotifyTokenResponse res)
    {
        return new TokenData()
        {
            Code = string.Empty,
            State = string.Empty,
            AccessToken = res.AccessToken,
            RefreshToken = res.RefreshToken,
            TokenType = res.TokenType,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(res.ExpiresIn),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }
}
