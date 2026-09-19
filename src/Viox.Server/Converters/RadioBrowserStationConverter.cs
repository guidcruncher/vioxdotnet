using Viox.Client.RadioBrowser.Models;
using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Server.Converters;

public sealed class RadioBrowserStationConverter : IMediaMetaDataConverter<Station>
{

    public string Source { get => "radiobrowser"; }
    public string Type { get => "station"; }

    public MediaMetaData Convert(object input)
    {
        return Convert((Station)input);
    }

    public MediaMetaData Convert(Station station)
    {
        return new MediaMetaData
        {
            Uri = station.Uri.ParseMediaUri(),
            Title = station.Name ?? string.Empty,
            Album = station.Country ?? string.Empty,
            Artist = station.State ?? string.Empty,
            Url = station.UrlResolved ?? station.Url ?? string.Empty,
            ImageUrl = station.Favicon ?? string.Empty
        };
    }

}
