using Microsoft.Extensions.Logging;

using Viox.Core.Models;
using Viox.Core.Storage;

namespace Viox.Core.Services;

/// <summary>
/// Application service encapsulating business rules and orchestrating interactions with storage.
/// </summary>
public class FavoritesService : IFavoritesService
{
    private readonly IFavoritesStorageEngine _storageEngine;
    private readonly ILogger<FavoritesService> _logger;

    public FavoritesService(
        IFavoritesStorageEngine storageEngine,
        ILogger<FavoritesService> logger)
    {
        _storageEngine = storageEngine ?? throw new ArgumentNullException(nameof(storageEngine));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public bool Exists(string rawUri)
    {
        _logger.LogDebug("Checking existence for favorite with RawUri: {RawUri}", rawUri);
        return _storageEngine.Exists(rawUri);
    }

    public IReadOnlyCollection<MediaMetaData> GetAllFavorites()
    {
        _logger.LogInformation("Fetching all favorites.");
        return _storageEngine.GetAll().OrderBy(m => m.Title).ToList();
    }

    public async Task<bool> AddFavoriteAsync(MediaMetaData item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        _logger.LogInformation("Adding favorite with RawUri: {RawUri}", item.RawUri);

        return await _storageEngine.AddAsync(item, cancellationToken);
    }

    public async Task<bool> RemoveFavoriteAsync(string rawUri, CancellationToken cancellationToken = default)
    {
        MediaUri? uri = MediaUriParser.ParseMediaUriValue(rawUri);
        if (uri is null) return false;

        _logger.LogInformation("Removing favorite with RawUri: {RawUri}", rawUri);
        return await _storageEngine.RemoveAsync(rawUri, cancellationToken);
    }

    public async Task ClearFavoritesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Clearing all favorites from storage.");
        await _storageEngine.ClearAsync(cancellationToken);
    }

    public MediaMetaData? GetByRawUri(string rawUri)
    {
        _logger.LogDebug("Retrieving favorite by RawUri: {RawUri}", rawUri);
        return _storageEngine.FindByRawUri(rawUri);
    }

    public IReadOnlyCollection<MediaMetaData> SearchFavorites(string query)
    {
        _logger.LogInformation("Searching favorites with query string: {Query}", query);
        return _storageEngine.SearchByRawUri(query);
    }

    /// <summary>
    /// Iterates over a collection of media metadata items, evaluates their current favorite state against storage,
    /// and returns the updated collection.
    /// </summary>
    /// <param name="items">The collection of media metadata items to update.</param>
    /// <returns>A list of media metadata items with updated favorite statuses.</returns>
    public IList<MediaMetaData> SetFavoriteStates(IEnumerable<MediaMetaData> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        _logger.LogInformation("Evaluating favorite states for media items.");

        HashSet<string> favoriteUris = _storageEngine.GetAll()
            .Select(f => f.RawUri)
            .Where(uri => !string.IsNullOrEmpty(uri))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        List<MediaMetaData> updatedItems = new();

        foreach (MediaMetaData item in items)
        {
            if (item is not null)
            {
                item.Favourite = !string.IsNullOrEmpty(item.RawUri) && favoriteUris.Contains(item.RawUri);
                updatedItems.Add(item);
            }
        }

        return updatedItems;
    }
}
