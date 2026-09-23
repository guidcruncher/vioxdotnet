using Viox.Core.Models;

namespace Viox.Server.Storage;

/// <summary>
/// Defines contracts for the fast, thread-safe, persisted favorites storage engine.
/// </summary>
public interface IFavoritesStorageEngine
{
    /// <summary>
    /// Checks if a favorite exists in storage by its RawUri identifier.
    /// </summary>
    bool Exists(string rawUri);

    /// <summary>
    /// Retrieves all stored favorites.
    /// </summary>
    IReadOnlyCollection<MediaMetaData> GetAll();

    /// <summary>
    /// Adds or updates a favorite item in storage.
    /// </summary>
    Task<bool> AddAsync(MediaMetaData item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a favorite item by its RawUri identifier.
    /// </summary>
    Task<bool> RemoveAsync(string rawUri, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all favorites from storage and disk persistence.
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Fast look-up for a favorite by its RawUri ($O(1)$ complexity).
    /// </summary>
    MediaMetaData? FindByRawUri(string rawUri);

    /// <summary>
    /// Searches for stored favorites matching a RawUri prefix or substring.
    /// </summary>
    IReadOnlyCollection<MediaMetaData> SearchByRawUri(string query);
}

