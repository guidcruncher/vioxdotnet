// File: MediaSourceResolverService.cs
namespace Viox.Core.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Viox.Core.Models;

/// <summary>
/// Dynamically resolves <see cref="IMediaSource"/> instances using Keyed Services.
/// </summary>
public class MediaSourceResolverService
{
    public readonly string[] Sources = ["podverse", "spotify", "tunein", "radiobrowser"];

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MediaSourceResolverService> _logger;

    public MediaSourceResolverService(
        IServiceProvider serviceProvider,
        ILogger<MediaSourceResolverService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    /// <summary>
    /// Fetches the registered keyed <see cref="IMediaSource"/> 
    /// </summary>
    public IMediaSource? ResolveMediaSource(string source)
    {
        return _serviceProvider.GetKeyedService<IMediaSource>(source);
    }

    public List<IMediaSource> GetSources()
    {
        return Sources
            .Select(source => _serviceProvider.GetKeyedService<IMediaSource>(source))
            .OfType<IMediaSource>()
            .ToList();
    }

    /// <summary>
    /// Fetches the registered keyed <see cref="IMediaSource"/> matching <see cref="MediaUri.Source"/> and resolves metadata.
    /// </summary>
    public async Task<MediaMetaData?> ResolveAsync(MediaUri uri, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(uri);

        IMediaSource? source = ResolveMediaSource(uri.Source);

        if (source is null)
        {
            _logger.LogError("No IMediaSource registered for source key '{Source}'", uri.Source);
            throw new InvalidOperationException($"Unsupported media source provider '{uri.Source}'.");
        }

        return await source.ResolveMetaData(uri, ct);
    }

}
