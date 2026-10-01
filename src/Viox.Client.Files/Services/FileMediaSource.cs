namespace Viox.Client.Files.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Viox.Core.Models;
using Viox.Core.Plugins;

/// <summary>
/// RadioBrowser implementation of <see cref="IMediaSource"/> registered under key "RadioBrowser".
/// </summary>
public class FileMediaSource : IMediaSource
{
    private readonly ILogger<FileMediaSource> _logger;
    private readonly IFileScanner _client;

    public string Source { get => "file"; }
    public string Title => "File";

    public Dictionary<string, string> Props { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Title"] = "File",
        ["Icon"] = "",
        ["Url"] = ""
    };

    public FileMediaSource(
        IFileScanner client,
        ILogger<FileMediaSource> logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(logger);
        _client = client;
        _logger = logger;
    }

    public async Task<MediaMetaData?> ResolveMetaData(MediaUri? uri, CancellationToken ct = default)
    {
        if (uri is null)
        {
            return null;
        }

        return (await _client.ScanDirectoryAsync(null, ct))
        .Where(t => t.RawUri == uri.ToString()).FirstOrDefault();
    }

    public async Task<PagedList<MediaMetaData>> Query(string query, int pageNumber, int limit, CancellationToken ct = default)
    {
        List<MediaMetaData> items = (await _client.ScanDirectoryAsync(null, ct))
            .Where(t => t.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

        int offset = (pageNumber - 1) * limit;
        return new PagedList<MediaMetaData>(items, offset, limit);
    }

    public async Task<IList<MediaMetaData>> ReadAsync(Dictionary<string, object>? parameters, CancellationToken cancellationToken = default)
    {
        return (await _client.ScanDirectoryAsync(null, cancellationToken)).ToList();
    }

    public async Task<IList<MediaMetaData>> ResolveChildItems(MediaUri? parent, string childType, CancellationToken ct)
    {
        return new List<MediaMetaData>();
    }

    public async Task<string> GetPlaybackUrl(MediaMetaData input, CancellationToken ct = default)
    {
        return input.Url;
    }

}
