using Viox.Client.TuneIn.Models;
using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Client.TuneIn.Services;

public sealed class TuneInStationDetailConverter : MediaMetaDataConverterBase, IMediaMetaDataConverter<StationDetail>
{

    public string Source { get => "tunein"; }
    public string Type { get => "station"; }


    public TuneInStationDetailConverter() : base()
    {
    }

    public MediaMetaData Convert(object input)
    {
        return Convert((StationDetail)input);
    }

    public MediaMetaData Convert(StationDetail station)
    {
        MediaMetaData metaData = new()
        {
            Uri = station.Station.Uri.ParseMediaUri(),
            Title = station.Station.Name ?? string.Empty,
            Album = station.Station.Slogan ?? string.Empty,
            Artist = station.Station.CallSign ?? string.Empty,
            Url = station.Audio.Url ?? string.Empty,
            ImageUrl = station.Station.Logo ?? string.Empty
        };
        metaData.Favourite = _favourites.Exists(metaData.RawUri);
        return Decorate(metaData);
    }

}
