
// File: MediaSourceResolver.cs
namespace Viox.Core.Services;

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Viox.Core.Models;

public class MediaSourceResolverService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MediaSourceResolverService> _logger;
    private readonly IEnumerable<IMediaSource> _sources;

    public MediaSourceResolverService(
        IServiceProvider serviceProvider,
        ILogger<MediaSourceResolverService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sources = serviceProvider.GetKeyedServices<IMediaSource>(KeyedService.AnyKey);

        int count = _sources.Count();
        if (count == 0)
        {
            _logger.LogWarning("Found no IMediaSource implementations.");
        }
        else
        {
            _logger.LogInformation("Found {Count} IMediaSource implementations.", count);
        }
    }

    public async Task<MediaMetaData?> ResolveMetaData(MediaUri? uri, CancellationToken ct)
    {
        if (uri is null)
        {
            return null;
        }

        IMediaSource? source = ResolveMediaSource(uri.Source);

        if (source is null)
        {
            return null;
        }

        return await source.ResolveMetaData(uri, ct);
    }

    /// <summary>
    /// Fetches the registered <see cref="IMediaSource"/> by matching its Source using LINQ.
    /// </summary>
    /// <param name="name">The name identifier of the converter to resolve.</param>
    /// <returns>The resolved converter instance, or <c>null</c> if not found.</returns>
    public IMediaSource? ResolveMediaSource(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _logger.LogDebug("Searching for converter with Source '{Name}'.", name);

        IMediaSource? match = _sources.FirstOrDefault(c =>
            string.Equals(c.Source, name, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            _logger.LogWarning("No media metadata converter found with Name '{Name}'.", name);
        }

        return match;
    }

}
