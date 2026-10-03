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

    private readonly string fileExtension = ".playlist.json";

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

        var safeName = GetSafeFilename(playlist.Title);
        var filePath = Path.Combine(_options.StorageDirectory, $"{safeName}");

        _logger.LogInformation("Saving playlist {PlaylistTitle} to {FilePath}", playlist.Title, filePath);

        var json = JsonSerializer.Serialize(playlist, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json, cancellationToken);
    }

    public async Task<MediaMetaDataPlaylist?> LoadPlaylistAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var safeName = GetSafeFilename(name);
        var filePath = Path.Combine(_options.StorageDirectory, $"{safeName}");

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Playlist file not found for name {PlaylistName} at {FilePath}", name, filePath);
            return null;
        }

        _logger.LogInformation("Loading playlist {PlaylistName} from {FilePath}", name, filePath);
        var json = await File.ReadAllTextAsync(filePath, cancellationToken);

        return JsonSerializer.Deserialize<MediaMetaDataPlaylist>(json, _jsonOptions);
    }

    public Task<IEnumerable<string>> GetPlaylistNamesAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(_options.StorageDirectory))
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }

        var files = Directory.GetFiles(_options.StorageDirectory, $"*{fileExtension}");
        var names = files.Select(Path.GetFileNameWithoutExtension).Where(n => n != null).Cast<string>();

        return Task.FromResult(names);
    }

    public Task<bool> DeletePlaylistAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var safeName = GetSafeFilename(name);
        var filePath = Path.Combine(_options.StorageDirectory, $"{safeName}");

        if (!File.Exists(filePath))
        {
            return Task.FromResult(false);
        }

        _logger.LogInformation("Deleting playlist {PlaylistName} at {FilePath}", name, filePath);
        File.Delete(filePath);
        return Task.FromResult(true);
    }

    private string GetSafeFilename(string title)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var filename = string.Concat(title.Select(c => invalidChars.Contains(c) ? '_' : c));
        return $"{filename}{fileExtension}";
    }

}
