using System.Collections.Concurrent;
using System.Text.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Core.Configuration;
using Viox.Core.Models;

namespace Viox.Core.Storage;

/// <summary>
/// Thread-safe in-memory storage engine backed by JSON disk persistence.
/// Optimized for fast retrieval by RawUri using concurrent hash-map indexing.
/// </summary>
public class FavoritesStorageEngine : IFavoritesStorageEngine
{
    private readonly ConcurrentDictionary<string, MediaMetaData> _favorites = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private readonly FavoritesStorageOptions _options;
    private readonly ILogger<FavoritesStorageEngine> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public FavoritesStorageEngine(
        IOptions<FavoritesStorageOptions> options,
        ILogger<FavoritesStorageEngine> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        LoadFromDisk();
    }

    public bool Exists(string rawUri)
    {
        if (string.IsNullOrWhiteSpace(rawUri))
        {
            return false;
        }

        return _favorites.ContainsKey(rawUri);
    }

    public IReadOnlyCollection<MediaMetaData> GetAll()
    {
        return _favorites.Values.ToList().AsReadOnly();
    }

    public async Task<bool> AddAsync(MediaMetaData item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);

        string rawUri = item.RawUri;
        if (string.IsNullOrWhiteSpace(rawUri))
        {
            _logger.LogWarning("Attempted to add MediaMetaData with empty RawUri.");
            return false;
        }

        item.Favourite = true;
        _favorites[rawUri] = item;

        if (_options.AutoSave)
        {
            await SaveToDiskAsync(cancellationToken);
        }

        return true;
    }

    public async Task<bool> RemoveAsync(string rawUri, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawUri))
        {
            return false;
        }

        if (_favorites.TryRemove(rawUri, out _))
        {
            if (_options.AutoSave)
            {
                await SaveToDiskAsync(cancellationToken);
            }
            return true;
        }

        return false;
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        _favorites.Clear();
        if (_options.AutoSave)
        {
            await SaveToDiskAsync(cancellationToken);
        }
    }

    public MediaMetaData? FindByRawUri(string rawUri)
    {
        if (string.IsNullOrWhiteSpace(rawUri))
        {
            return null;
        }

        _favorites.TryGetValue(rawUri, out var item);
        return item;
    }

    public IReadOnlyCollection<MediaMetaData> SearchByRawUri(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<MediaMetaData>();
        }

        return _favorites.Values
            .Where(x => x.RawUri.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }

    private void LoadFromDisk()
    {
        try
        {
            if (!File.Exists(_options.FilePath))
            {
                _logger.LogInformation("Favorites persistence file not found at {FilePath}. Starting empty.", _options.FilePath);
                return;
            }

            using var stream = File.OpenRead(_options.FilePath);
            var items = JsonSerializer.Deserialize<List<MediaMetaData>>(stream, _jsonOptions);

            if (items is not null)
            {
                _favorites.Clear();
                foreach (var item in items)
                {
                    if (!string.IsNullOrWhiteSpace(item.RawUri))
                    {
                        item.Favourite = true;
                        _favorites[item.RawUri] = item;
                    }
                }
                _logger.LogInformation("Successfully loaded {Count} favorites from {FilePath}.", _favorites.Count, _options.FilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load favorites from path: {FilePath}", _options.FilePath);
        }
    }

    private async Task SaveToDiskAsync(CancellationToken cancellationToken)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            string? directory = Path.GetDirectoryName(_options.FilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var snapshot = _favorites.Values.ToList();
            await using var stream = File.Create(_options.FilePath);
            await JsonSerializer.SerializeAsync(stream, snapshot, _jsonOptions, cancellationToken);
            _logger.LogDebug("Successfully saved {Count} favorites to {FilePath}.", snapshot.Count, _options.FilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save favorites to path: {FilePath}", _options.FilePath);
        }
        finally
        {
            _fileLock.Release();
        }
    }
}

