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
    private readonly MediaMetaDataConverterResolver _resolver;

    public string Source { get => "librespot"; }

    public LibrespotMediaSource(
        ILibrespotRestClient client,
        ILogger<LibrespotMediaSource> logger,
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

        IMediaMetaDataConverterBase? converter = _resolver.ResolveConverter("librespot:*");
        if (converter is not null)
        {
            return converter.Convert(status.Track);
        }
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
