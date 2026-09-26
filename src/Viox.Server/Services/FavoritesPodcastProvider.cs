namespace Viox.Server.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Viox.Core.Models;
using Viox.Core.Services;

/// <summary>
/// Provides podcasts from favorite media items.
/// </summary>
public sealed class FavoritesPodcastProvider : IPodcastProvider
{
    private readonly IFavoritesService _service;

    public FavoritesPodcastProvider(IFavoritesService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public Task<IEnumerable<MediaMetaData>> GetPodcastsToDownloadAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<MediaMetaData> res = _service.GetAllFavorites();

        IEnumerable<MediaMetaData> items = res.Where(item =>
            item.Uri?.Source == "podverse" && item.Uri?.Type == "podcast");

        return Task.FromResult(items);
    }
}
