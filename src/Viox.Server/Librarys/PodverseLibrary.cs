// File: PodverseLibrary.cs
namespace Viox.Server.Librarys;

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Viox.Client.Podverse.Services;
using Viox.Core.Models;
using Viox.Core.Services;

public class PodverseLibrary : ILibrary
{
    private readonly IPodverseClient _client;
    private readonly ILogger<PodverseLibrary> _logger;

    public PodverseLibrary(
        IPodverseClient client,
        ILogger<PodverseLibrary> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Source => "podverse";

    public async Task<IList<MediaMetaData>> ReadAsync(Dictionary<string, object>? parameters, CancellationToken cancellationToken = default)
    {
        List<MediaMetaData> res = new();

        return res;
    }

}
