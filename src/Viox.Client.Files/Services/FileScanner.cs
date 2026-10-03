namespace Viox.Client.Files.Services;

using System;
using System.Collections.Generic;
using System.IO;

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Client.Files.Configuration;
using Viox.Core.Models;
using Viox.Core.Services;
using Viox.Core.Utilities;

public class FileScanner(
    ILogger<FileScanner> logger,
    IFavoritesService favourites,
    IOptions<MediaScannerOptions> options) : IFileScanner
{
    private readonly MediaScannerOptions _options = options.Value;

    public async Task<IReadOnlyList<MediaMetaData>> ScanDirectoryAsync(
        string? overrideDirectoryPath = null,
        CancellationToken cancellationToken = default)
    {
        string targetPath = string.IsNullOrWhiteSpace(overrideDirectoryPath)
            ? _options.DefaultDirectoryPath
            : overrideDirectoryPath;

        if (string.IsNullOrWhiteSpace(targetPath))
        {
            logger.LogError("Target directory path was not provided and configuration default is empty.");
            throw new ArgumentException("Directory path must be provided either via parameter or configuration options.", nameof(overrideDirectoryPath));
        }

        if (!Directory.Exists(targetPath))
        {
            logger.LogWarning("Specified directory does not exist: {DirectoryPath}", targetPath);
            return Array.Empty<MediaMetaData>();
        }

        logger.LogInformation("Starting media metadata scan in path: {DirectoryPath}", targetPath);

        SearchOption searchOption = _options.SearchSubdirectories
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly;

        DirectoryInfo directoryInfo = new(targetPath);
        List<MediaMetaData> results = [];

        try
        {
            IEnumerable<FileInfo> files = await Task.Run(() => directoryInfo.EnumerateFiles("*", searchOption), cancellationToken);

            foreach (FileInfo file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_options.SupportedExtensions.Contains(file.Extension))
                {
                    string filename = file.FullName;

                    MediaUri uri = new()
                    {
                        Source = "file",
                        Type = "track",
                        Id = filename.ToXxHash64Hex()
                    };

                    MediaMetaData metadata = new()
                    {
                        Uri = uri,
                        Title = file.Name,
                        Url = filename,
                        Album = "",
                        Artist = "",
                        ImageUrl = "/file.png"
                    };
                    metadata.Favourite = favourites.Exists(metadata.RawUri);
                    results.Add(metadata);
                }
            }

            logger.LogInformation("Scan complete for {DirectoryPath}. Found {Count} matching media items.", targetPath, results.Count);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Media scan was canceled for directory: {DirectoryPath}", targetPath);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while scanning directory: {DirectoryPath}", targetPath);
            throw;
        }

        return results;
    }

    private static string ResolveContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".mp4" => "video/mp4",
            ".mkv" => "video/x-matroska",
            ".avi" => "video/x-msvideo",
            ".mov" => "video/quicktime",
            ".wmv" => "video/x-ms-wmv",
            ".mp3" => "audio/mpeg",
            ".flac" => "audio/flac",
            ".wav" => "audio/wav",
            ".aac" => "audio/aac",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}
