using Viox.Client.Librespot.Models;
using Viox.Core.Models;
using Viox.Core.Playlists;
using Viox.Core.Services;

namespace Viox.Client.Librespot.Services;

public sealed class LibrespotConverter : MediaMetaDataConverterBase, IMediaMetaDataConverter<ApiTrack>
{

    public string Source { get => "librespot"; }
    public string Type { get => ""; }

    public LibrespotConverter(IPlaylistIndexService indexer, IFavoritesService favourites) : base(indexer, favourites)
    {
    }

    public MediaMetaData Convert(object input)
    {
        return Convert((ApiTrack)input);
    }

    public MediaMetaData Convert(ApiTrack input)
    {
        MediaMetaData metaData = new()
        {
            Uri = input.Uri.ParseMediaUri(),
            Title = input.Name ?? string.Empty,
            Album = input.AlbumName ?? string.Empty,
            Artist = input.ArtistNames is not null ? string.Join(", ", input.ArtistNames) : string.Empty,
            Url = input.Uri,
            ImageUrl = input.AlbumCoverUrl ?? string.Empty,
            Duration = input.Duration == 0 ? 0 : (input.Duration / 1000)
        };

        return Decorate(metaData);
    }

}
