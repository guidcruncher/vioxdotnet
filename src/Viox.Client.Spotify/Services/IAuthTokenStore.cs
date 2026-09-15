using Viox.Client.Spotify.Models;

namespace Viox.Client.Spotify.Services;

public interface IAuthTokenStore
{
    Task SaveTokenAsync(TokenData tokenData, CancellationToken cancellationToken = default);
    Task<TokenData?> GetTokenAsync(CancellationToken cancellationToken = default);
    Task ClearTokenAsync(CancellationToken cancellationToken = default);
}
