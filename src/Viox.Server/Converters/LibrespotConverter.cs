using Viox.Client.Librespot.Models;
using Viox.Core.Models;
using Viox.Core.Services;
using Viox.Server.Services;

namespace Viox.Server.Converters;

public sealed class LibrespotConverter : IMediaMetaDataConverter<ApiTrack>
{

    public string Source { get => "librespot"; }
    public string Type { get => ""; }

    private readonly IFavoritesService _favourites;

    public LibrespotConverter(IFavoritesService favourites)
    {
        _favourites = favourites;
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

        metaData.Favourite = _favourites.Exists(metaData.RawUri);
        return metaData;
    }

}
