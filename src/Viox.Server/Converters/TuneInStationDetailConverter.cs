using Viox.Client.TuneIn.Models;
using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Server.Converters;

public sealed class TuneInStationDetailConverter : IMediaMetaDataConverter<StationDetail>
{

    public string Source { get => "tunein"; }
    public string Type { get => "station"; }

    public MediaMetaData Convert(object input)
    {
        return Convert((StationDetail)input);
    }

    public MediaMetaData Convert(StationDetail station)
    {
        return new MediaMetaData
        {
            Uri = station.Station.Uri.ParseMediaUri(),
            Title = station.Station.Name ?? string.Empty,
            Album = station.Station.Slogan ?? string.Empty,
            Artist = station.Station.CallSign ?? string.Empty,
            Url = station.Audio.Url ?? string.Empty,
            ImageUrl = station.Station.Logo ?? string.Empty
        };
    }

}
