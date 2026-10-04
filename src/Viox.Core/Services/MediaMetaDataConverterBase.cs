namespace Viox.Core.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Viox.Core.Models;
using Viox.Core.Playlists;

public class MediaMetaDataConverterBase
{
    private readonly IPlaylistIndexService _indexer;
    private readonly IFavoritesService _favourites;

    public MediaMetaDataConverterBase(IPlaylistIndexService indexer, IFavoritesService favourites)
    {
_indexer=indexer;
_favourites=favourites;
    }

    public virtual MediaMetaData Decorate(MediaMetaData item)
    {
        ArgumentNullException.ThrowIfNull(item);

        MediaMetaData metadata = item;

        metadata.InPlaylist = _indexer.ContainsUri(item.RawUri);
        metadata.Favourite = _favourites.Exists(item.RawUri);

        return metadata;
    }
}
