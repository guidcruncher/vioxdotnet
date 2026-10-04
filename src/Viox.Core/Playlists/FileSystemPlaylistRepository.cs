// File: FileSystemPlaylistRepository.cs
using System.Text.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Core.Configuration;
using Viox.Core.Models;

namespace Viox.Core.Playlists;

/// <summary>
/// File system-based implementation of <see cref="IPlaylistRepository"/>.
/// </summary>
public class FileSystemPlaylistRepository : IPlaylistRepository
{
    private readonly PlaylistOptions _options;
    private readonly ILogger<FileSystemPlaylistRepository> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    private readonly string fileExtension = ".json";

    public FileSystemPlaylistRepository(
        IOptions<PlaylistOptions> options,
        ILogger<FileSystemPlaylistRepository> logger)
    {
        _options = options.Value;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        Directory.CreateDirectory(_options.StorageDirectory);
    }

    public async Task SavePlaylistAsync(MediaMetaDataPlaylist playlist, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(playlist);

        var safeName = GetSafeFilename(playlist.Id);
        var filePath = Path.Combine(_options.StorageDirectory, $"{safeName}");

        _logger.LogInformation("Saving playlist {PlaylistTitle} to {FilePath}", playlist.Title, filePath);

        var json = JsonSerializer.Serialize(playlist, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json, cancellationToken);
    }

    public async Task<MediaMetaDataPlaylist?> LoadPlaylistAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var safeName = GetSafeFilename(id);
        var filePath = Path.Combine(_options.StorageDirectory, $"{safeName}");

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Playlist file not found for name {PlaylistName} at {FilePath}", id, filePath);
            return null;
        }

        _logger.LogInformation("Loading playlist {PlaylistName} from {FilePath}", id, filePath);
        var json = await File.ReadAllTextAsync(filePath, cancellationToken);

        return JsonSerializer.Deserialize<MediaMetaDataPlaylist>(json, _jsonOptions);
    }

    public async Task<Dictionary<string, string>> GetPlaylistNamesAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_options.StorageDirectory))
        {
            _logger.LogWarning("Storage directory not found '{folder}'", _options.StorageDirectory);
            return new Dictionary<string, string>();
        }

        var files = Directory.GetFiles(_options.StorageDirectory, $"*{fileExtension}");
        _logger.LogInformation("Found {files} count in folder '{folder}' filter '{filter}'", files.Count(), _options.StorageDirectory, $"*{fileExtension}");
        Dictionary<string, string> res = new();

        foreach (string f in files)
        {
            string id = Path.GetFileNameWithoutExtension(f);
            MediaMetaDataPlaylist? pl = await LoadPlaylistAsync(id, cancellationToken);
            if (pl is not null)
            {
                res.Add(pl.Id, pl.Title);
            }
        }

        return res;
    }

    public Task<bool> DeletePlaylistAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var safeName = GetSafeFilename(id);
        var filePath = Path.Combine(_options.StorageDirectory, $"{safeName}");

        if (!File.Exists(filePath))
        {
            return Task.FromResult(false);
        }

        _logger.LogInformation("Deleting playlist {PlaylistName} at {FilePath}", id, filePath);
        File.Delete(filePath);
        return Task.FromResult(true);
    }

    private string GetSafeFilename(string id)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var filename = string.Concat(id.Select(c => invalidChars.Contains(c) ? '_' : c));
        return $"{filename}{fileExtension}";
    }

}
