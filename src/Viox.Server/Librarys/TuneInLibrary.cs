// File: TuneInLibrary.cs
namespace Viox.Server.Librarys;

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Viox.Client.TuneIn.Models;
using Viox.Client.TuneIn.Services;
using Viox.Core.Models;
using Viox.Core.Services;

public class TuneInLibrary : ILibrary
{
    private readonly ITuneInClient _client;
    private readonly ILogger<TuneInLibrary> _logger;

    public TuneInLibrary(
        ITuneInClient client,
        ILogger<TuneInLibrary> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Source => "tunein";

    private List<MediaMetaData> ParseOutlineList(List<TuneInOutline> outlines)
    {
        List<MediaMetaData> res = new();
        foreach (TuneInOutline outline in outlines)
        {
            if (outline.Key == "stations")
            {
                if (outline.Children is not null)
                {
                    List<MediaMetaData> stations = ParseOutlineList(outline.Children);
                    if (stations.Any())
                    {
                        res.AddRange(stations);
                    }
                }
                continue;
            }

            if (outline.Type is not null)
            {
                string uri = outline.Type switch
                {
                    "audio" => $"tunein:station:{outline.GuideId}",
                    "link" => $"tunein:link:{outline.GuideId}",
                    _ => $"tunein:{outline.Type}:{outline.GuideId}"
                };

                MediaMetaData metaData = new()
                {
                    Uri = MediaUriParser.ParseMediaUriValue(uri),
                    Title = outline.Text ?? "",
                    ImageUrl = outline.Image ?? "",
                    Album = "",
                    Artist = "",
                    Url = outline.Url ?? ""
                };

                res.Add(metaData);
            }
        }
        return res;
    }

    public async Task<IList<MediaMetaData>> ReadAsync(Dictionary<string, object>? parameters, CancellationToken cancellationToken = default)
    {
        List<MediaMetaData> res = new();
        string id = parameters?.GetValueOrDefault("id")?.ToString() ?? "r0";
        TuneInResponse<TuneInOutline>? response = await _client.BrowseAsync(null, id, null, cancellationToken);

        if (response is null)
        {
            return res;
        }

        if (response.Body is not null)
        {
            res = ParseOutlineList(response.Body);
        }

        return res;
    }

}
