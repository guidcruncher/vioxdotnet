using Viox.Core.Playlists;
using Viox.Core.Models;

namespace Viox.Core.Services;

public class MediaMetaDataConverterBase {

private readonly IPlaylistIndexService _indexer;
private readonly IFavoritesService _favourites;

    public MediaMetaDataConverterBase(IPlaylistIndexService indexer, IFavoritesService favourites)
    {
        _favourites = favourites;
        _indexer= indexer;
     }

public MediaMetaData Decorate(MediaMetaData item) 
{  
MediaMetaData  metadata = item;

metadata.InPlaylist = _indexer.ContainsUri(item.RawUri);
metaData.Favourite = _favourites.Exists(item.RawUri);
return metadata;
}

}
