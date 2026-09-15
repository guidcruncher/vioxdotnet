// File: LibrespotMediaSource.cs
namespace Viox.Client.Librespot.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Viox.Client.Librespot.Models;
using Viox.Core.Models;
using Viox.Core.Services;

/// <summary>
/// Librespot implementation of <see cref="IMediaSource"/> registered under key "Librespot".
/// </summary>
public class LibrespotMediaSource : IMediaSource
{
    private readonly ILogger<LibrespotMediaSource> _logger;
    private readonly ILibrespotRestClient _client;

    public string Source { get => "librespot"; }

    public LibrespotMediaSource(
        ILibrespotRestClient client,
        ILogger<LibrespotMediaSource> logger)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MediaMetaData?> ResolveMetaData(MediaUri? uri, CancellationToken ct = default)
    {
        ApiStatus? status = await _client.GetStatusAsync(ct);

        if (status?.Track is null)
        {
            _logger.LogWarning("Unable to resolve metadata: Librespot status or track is null.");
            return null;
        }

        if (string.IsNullOrEmpty(status.Track.Uri))
        {
            _logger.LogWarning("Unable to resolve metadata: Track URI is null or empty.");
            return null;
        }

        MediaUri? mediaUri = status.Track.Uri.ParseMediaUri();
        if (mediaUri is null)
        {
            _logger.LogWarning("Failed to parse MediaUri from track URI '{TrackUri}'.", status.Track.Uri);
            return null;
        }

        MediaMetaData metaData = new()
        {
            Uri = mediaUri,
            Title = status.Track.Name ?? string.Empty,
            Album = status.Track.AlbumName ?? string.Empty,
            Artist = status.Track.ArtistNames is not null ? string.Join(", ", status.Track.ArtistNames) : string.Empty,
            Url = status.Track.Uri,
            ImageUrl = status.Track.AlbumCoverUrl ?? string.Empty
        };

        return metaData;
    }

    public async Task<PagedList<MediaMetaData>> Query(string query, int pageNumber, int limit, CancellationToken ct = default)
    {
        return new PagedList<MediaMetaData>();
    }
}
