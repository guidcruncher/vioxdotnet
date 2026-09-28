
// File: MediaSourceResolver.cs
namespace Viox.Core.Services;

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Viox.Core.Models;
using Viox.Core.Plugins;

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

    /// <summary>
    /// Iterates an <see cref="IEnumerable{IMediaSource}"/> and creates a <see cref="Dictionary{TKey, TValue}"/> 
    /// containing <see cref="IMediaSource.Source"/> as keys and <see cref="IMediaSource.Title"/> as values.
    /// </summary>
    /// <param name="keyComparer">Optional string comparer for dictionary keys. Defaults to <see cref="StringComparer.Ordinal"/>.</param>
    /// <returns>A dictionary containing mapped source-to-title pairs.</returns>
    public Dictionary<string, string> GetInstalledSources(
        IEqualityComparer<string>? keyComparer = null)
    {
        IEqualityComparer<string> comparer = keyComparer ?? StringComparer.Ordinal;

        if (_sources is null)
        {
            _logger.LogDebug("Provided media source collection was null. Returning empty dictionary.");
            return new Dictionary<string, string>(comparer);
        }

        var sourceMap = new Dictionary<string, string>(comparer);

        foreach (var item in _sources)
        {
            if (item is null)
            {
                _logger.LogWarning("Encountered null IMediaSource item during dictionary mapping. Skipping item.");
                continue;
            }

            if (string.IsNullOrEmpty(item.Source))
            {
                _logger.LogWarning("Skipping IMediaSource item with empty or null Source property. Title: {Title}", item.Title);
                continue;
            }

            if (item.Source == "librespot")
            {
                continue;
            }

            string titleValue = item.Title ?? string.Empty;

            if (!sourceMap.TryAdd(item.Source, titleValue))
            {
                _logger.LogWarning(
                    "Duplicate Source key detected: '{SourceKey}'. Existing Title '{ExistingTitle}' retained; new Title '{IgnoredTitle}' ignored.",
                    item.Source,
                    sourceMap[item.Source],
                    titleValue
                );
            }
        }

        return sourceMap
    .OrderBy(pair => pair.Value)
    .ToDictionary(pair => pair.Key, pair => pair.Value);

    }

    public Dictionary<string, Dictionary<string, string>> GetInstalledSourceProps()
    {

        if (_sources is null)
        {
            _logger.LogDebug("Provided media source collection was null. Returning empty dictionary.");
            return new Dictionary<string, Dictionary<string, string>>();
        }

        var sourceMap = new Dictionary<string, Dictionary<string, string>>();

        foreach (var item in _sources)
        {
            if (item is null)
            {
                _logger.LogWarning("Encountered null IMediaSource item during dictionary mapping. Skipping item.");
                continue;
            }

            if (string.IsNullOrEmpty(item.Source))
            {
                _logger.LogWarning("Skipping IMediaSource item with empty or null Source property. Title: {Title}", item.Title);
                continue;
            }

            if (item.Source == "librespot")
            {
                continue;
            }

            if (!sourceMap.TryAdd(item.Source, item.Props))
            {
                _logger.LogWarning(
                    "Duplicate Source key detected: '{SourceKey}'.",
                    item.Source
                );
            }
        }

        return sourceMap
    .OrderBy(pair => pair.Value["Title"])
    .ToDictionary(pair => pair.Key, pair => pair.Value);

    }

}
