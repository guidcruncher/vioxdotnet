// File: RadioBrowserLibrary.cs
namespace Viox.Server.Librarys;

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Viox.Client.RadioBrowser.Services;
using Viox.Core.Models;
using Viox.Core.Services;

public class RadioBrowserLibrary : ILibrary
{
    private readonly IRadioBrowserClient _client;
    private readonly ILogger<RadioBrowserLibrary> _logger;

    public RadioBrowserLibrary(
        IRadioBrowserClient client,
        ILogger<RadioBrowserLibrary> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Source => "radiobrowser";

    public async Task<IList<MediaMetaData>> ReadAsync(Dictionary<string, object>? parameters, CancellationToken cancellationToken = default)
    {
        List<MediaMetaData> res = new();

        return res;
    }

}
