using Viox.Core.Models;

namespace Viox.Core.Services;

/// <summary>
/// Service abstraction interface managing high-level favorites operations.
/// </summary>
public interface IFavoritesService
{
    /// <summary>
    /// Checks whether a favorite item exists by its RawUri.
    /// </summary>
    bool Exists(string rawUri);

    IReadOnlyCollection<MediaMetaData> GetAllFavorites();
    Task<bool> AddFavoriteAsync(MediaMetaData item, CancellationToken cancellationToken = default);
    Task<bool> RemoveFavoriteAsync(string rawUri, CancellationToken cancellationToken = default);
    Task ClearFavoritesAsync(CancellationToken cancellationToken = default);
    MediaMetaData? GetByRawUri(string rawUri);
    IReadOnlyCollection<MediaMetaData> SearchFavorites(string query);
    IList<MediaMetaData> SetFavoriteStates(IEnumerable<MediaMetaData> items);
}
