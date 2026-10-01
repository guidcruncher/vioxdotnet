// File: YoutubeMediaSource.cs
namespace Viox.Client.Youtube.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Viox.Core.Models;
using Viox.Core.Plugins;
using Viox.Core.Services;

/// <summary>
/// Youtube implementation of <see cref="IMediaSource"/> registered under key "Youtube".
/// </summary>
public class YoutubeMediaSource : IMediaSource
{
    private readonly ILogger<YoutubeMediaSource> _logger;
    private readonly IYoutubeMusicClient _client;
    private readonly MediaMetaDataConverterResolver _resolver;

    public string Source { get => "youtube"; }
    public string Title => "Youtube Music";

    public Dictionary<string, string> Props { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Title"] = "Youtube Music",
        ["Icon"] = "",
        ["Url"] = ""
    };

    public YoutubeMediaSource(
        IYoutubeMusicClient client,
        ILogger<YoutubeMediaSource> logger,
        MediaMetaDataConverterResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
        _resolver = resolver;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MediaMetaData?> ResolveMetaData(MediaUri? uri, CancellationToken ct = default)
    {
        return null;
    }

    public async Task<PagedList<MediaMetaData>> Query(string query, int pageNumber, int limit, CancellationToken ct = default)
    {
        return new PagedList<MediaMetaData>();
    }

    public async Task<IList<MediaMetaData>> ReadAsync(Dictionary<string, object>? parameters, CancellationToken cancellationToken = default)
    {
        return new List<MediaMetaData>();
    }

    public async Task<IList<MediaMetaData>> ResolveChildItems(MediaUri? parent, string childType, CancellationToken ct)
    {
        return new List<MediaMetaData>();
    }
}
