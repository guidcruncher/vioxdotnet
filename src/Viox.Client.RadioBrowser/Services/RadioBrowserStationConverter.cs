using Viox.Client.RadioBrowser.Models;
using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Client.RadioBrowser.Services;

public sealed class RadioBrowserStationConverter : IMediaMetaDataConverter<Station>
{

    public string Source { get => "radiobrowser"; }
    public string Type { get => "station"; }

    private readonly IFavoritesService _favourites;

    public RadioBrowserStationConverter(IFavoritesService favourites)
    {
        _favourites = favourites;
    }

    public MediaMetaData Convert(object input)
    {
        return Convert((Station)input);
    }

    public MediaMetaData Convert(Station station)
    {
        MediaMetaData metaData = new()
        {
            Uri = station.Uri.ParseMediaUri(),
            Title = station.Name ?? string.Empty,
            Album = station.CountryCode ?? string.Empty,
            Artist = station.State ?? string.Empty,
            Url = station.UrlResolved ?? station.Url ?? string.Empty,
            ImageUrl = station.Favicon ?? string.Empty
        };
        metaData.Favourite = _favourites.Exists(metaData.RawUri);
        return metaData;
    }

}
