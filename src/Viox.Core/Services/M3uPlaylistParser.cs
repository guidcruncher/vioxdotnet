// File: M3uPlaylistParser.cs
namespace Viox.Core.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Core.Configuration;
using Viox.Core.Models;
using Viox.Core.Utilities;

/// <summary>
/// Provides functionality to parse M3U/M3U8 playlist content from streams, files, or URLs.
/// Targets .NET 10 and utilizes standard Microsoft Extensions libraries.
/// </summary>
public class M3uPlaylistParser : IM3uPlaylistParser
{
    // Refined regex allowing attributes before or after the comma, properly capturing key-value options and channel title
    private static readonly Regex ExtInfRegex = new Regex(
        @"^#EXTINF:\s*(?<duration>-?\d+(?:\.\d+)?)(?<attributes>(?:\s+[a-zA-Z0-9\-_]+=""[^""]*"")*)\s*,?\s*(?<title>.*)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex AttributeRegex = new Regex(
        @"(?<key>[a-zA-Z0-9\-_]+)=\""(?<value>[^\""]*)\""",
        RegexOptions.Compiled);

    private readonly HttpClient _httpClient;
    private readonly ILogger<M3uPlaylistParser> _logger;
    private readonly M3uPlaylistParserOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="M3uPlaylistParser"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used for fetching remote playlists.</param>
    /// <param name="options">Configuration options for parser operation.</param>
    /// <param name="logger">The logger for diagnostics and runtime reporting.</param>
    public M3uPlaylistParser(
        HttpClient httpClient,
        IOptions<M3uPlaylistParserOptions> options,
        ILogger<M3uPlaylistParser> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? new M3uPlaylistParserOptions();
    }

    /// <inheritdoc />
    public async Task<MediaMetaData[]> ParseAsync(string id, Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
        {
            _logger.LogError("The provided stream is not readable.");
            throw new ArgumentException("Stream must be readable.", nameof(stream));
        }

        var cipher = new CompactStringCipher();
        var items = new List<MediaMetaData>();
        using var reader = new StreamReader(stream, leaveOpen: true);
        var encodedId = id.ToXxHash64Hex();

        string? currentTitle = null;
        string? currentArtist = null;
        string? currentAlbum = null;
        string? currentImageUrl = null;
        double? currentDuration = null;

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false)) != null)
        {
            line = line.Trim();

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.StartsWith("#EXTINF:", StringComparison.OrdinalIgnoreCase))
            {
                ParseExtInfLine(
                    line,
                    out currentDuration,
                    out currentTitle,
                    out currentArtist,
                    out currentAlbum,
                    out currentImageUrl);

                continue;
            }

            if (line.StartsWith('#'))
            {
                // Skip unhandled metadata tags or comments
                continue;
            }

            // Treat line as a URI target
            if (_options.RequireAbsoluteUri && !Uri.IsWellFormedUriString(line, UriKind.Absolute))
            {
                _logger.LogWarning("Skipping line because it is not an absolute URI: {Line}", line);
                ResetPendingState(ref currentDuration, ref currentTitle, ref currentArtist, ref currentAlbum, ref currentImageUrl);
                continue;
            }

            var title = string.IsNullOrWhiteSpace(currentTitle) ? Path.GetFileNameWithoutExtension(line) : currentTitle;
            var artist = currentArtist ?? string.Empty;
            var album = currentAlbum ?? string.Empty;
            var imageUrl = currentImageUrl ?? string.Empty;

            var uri = new MediaUri
            {
                Source = "playlist",
                Type = "media",
                Id = encodedId,
                SecondaryId = line.ToXxHash64Hex()
            };

            var metadata = new MediaMetaData
            {
                Uri = uri,
                Title = title,
                Artist = artist,
                Album = album,
                Url = line,
                ImageUrl = imageUrl,
                Duration = currentDuration
            };

            items.Add(metadata);

            ResetPendingState(ref currentDuration, ref currentTitle, ref currentArtist, ref currentAlbum, ref currentImageUrl);
        }

        _logger.LogInformation("Successfully parsed {Count} playlist items.", items.Count);
        return items.ToArray();
    }

    /// <inheritdoc />
    public async Task<MediaMetaData[]> ParseFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            _logger.LogError("M3U file not found at path: {FilePath}", filePath);
            throw new FileNotFoundException("M3U playlist file was not found.", filePath);
        }

        _logger.LogDebug("Opening M3U file stream for reading: {FilePath}", filePath);
        await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        return await ParseAsync(filePath, fileStream, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<MediaMetaData[]> ParseFromUrlAsync(string playlistUrl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(playlistUrl);

        if (!Uri.TryCreate(playlistUrl, UriKind.Absolute, out var uri))
        {
            _logger.LogError("Invalid URL format supplied: {Url}", playlistUrl);
            throw new ArgumentException("Invalid absolute URL provided.", nameof(playlistUrl));
        }

        _logger.LogDebug("Fetching remote M3U playlist from: {Url}", playlistUrl);
        using var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return await ParseAsync(uri.ToString(), stream, cancellationToken).ConfigureAwait(false);
    }

    private static void ParseExtInfLine(
        string line,
        out double? duration,
        out string? title,
        out string? artist,
        out string? album,
        out string? imageUrl)
    {
        duration = null;
        title = null;
        artist = null;
        album = null;
        imageUrl = null;

        var match = ExtInfRegex.Match(line);
        if (!match.Success)
        {
            return;
        }

        if (double.TryParse(match.Groups["duration"].Value, out var parsedDuration) && parsedDuration >= 0)
        {
            duration = parsedDuration;
        }

        var attributesRaw = match.Groups["attributes"].Value;
        if (!string.IsNullOrWhiteSpace(attributesRaw))
        {
            var attrMatches = AttributeRegex.Matches(attributesRaw);
            foreach (Match attrMatch in attrMatches)
            {
                var key = attrMatch.Groups["key"].Value;
                var value = attrMatch.Groups["value"].Value;

                if (string.Equals(key, "tvg-logo", StringComparison.OrdinalIgnoreCase))
                {
                    imageUrl = value;
                }
                else if (string.Equals(key, "group-title", StringComparison.OrdinalIgnoreCase))
                {
                    album = value;
                }
                else if (string.Equals(key, "tvg-name", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(title))
                {
                    title = value;
                }
            }
        }

        var titleRaw = match.Groups["title"].Value.Trim();
        if (!string.IsNullOrWhiteSpace(titleRaw))
        {
            var parts = titleRaw.Split(" - ", 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                artist = parts[0].Trim();
                title = parts[1].Trim();
            }
            else
            {
                title = titleRaw;
            }
        }
    }

    private static void ResetPendingState(
        ref double? duration,
        ref string? title,
        ref string? artist,
        ref string? album,
        ref string? imageUrl)
    {
        duration = null;
        title = null;
        artist = null;
        album = null;
        imageUrl = null;
    }
}
