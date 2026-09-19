namespace Viox.Core.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Core.Configuration;
using Viox.Core.Models;

/// <summary>
/// Provides unified search capabilities across all registered <see cref="IMediaSource"/> implementations.
/// </summary>
public class MediaSearchService
{
    private readonly IReadOnlyDictionary<string, IMediaSource> _sources;
    private readonly MediaSearchOptions _options;
    private readonly ILogger<MediaSearchService> _logger;

    public MediaSearchService(
        IServiceProvider serviceProvider,
        IEnumerable<IMediaSource> unkeyedSources,
        IOptions<MediaSearchOptions> options,
        ILogger<MediaSearchService> logger)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(unkeyedSources);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _options = options.Value;

        // KeyedService.AnyKey is a static property and cannot be passed to attribute constructors.
        // Resolve all keyed instances programmatically via IServiceProvider.
        var keyedSources = serviceProvider.GetKeyedServices<IMediaSource>(KeyedService.AnyKey);

        var combinedSources = unkeyedSources
            .Concat(keyedSources)
            .Distinct();

        var sourcesDict = new Dictionary<string, IMediaSource>(StringComparer.OrdinalIgnoreCase);

        foreach (var source in combinedSources)
        {
            if (source.Source == "librespot")
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(source.Source))
            {
                _logger.LogWarning(
                    "Skipping media source of type '{TypeName}' because its Source property is null or empty.",
                    source.GetType().FullName);
                continue;
            }

            if (!sourcesDict.TryAdd(source.Source, source))
            {
                _logger.LogWarning(
                    "Duplicate media source identifier '{Source}' detected from type '{TypeName}'. Skipping duplicate.",
                    source.Source,
                    source.GetType().FullName);
            }
        }

        _sources = sourcesDict;

        _logger.LogInformation(
            "Initialized {ServiceName} with {SourceCount} registered media sources.",
            nameof(MediaSearchService),
            _sources.Count);
    }

    /// <summary>
    /// Executes a keyword search query concurrently across all available media sources.
    /// </summary>
    /// <param name="query">The search term or keyword to query.</param>
    /// <param name="pageNumber">The 1-based page index for pagination.</param>
    /// <param name="limit">The maximum number of items per page per source.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A dictionary containing search results mapped by each media source key.</returns>
    public async Task<IReadOnlyDictionary<string, PagedList<MediaMetaData>>> QueryAllSourcesAsync(
        string query,
        int pageNumber,
        int limit,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        _logger.LogInformation(
            "Broadcasting search query '{Query}' across {SourceCount} sources.",
            query,
            _sources.Count);

        var searchTasks = _sources.Select(async pair =>
        {
            var sourceKey = pair.Key;
            var source = pair.Value;

            try
            {
                var result = await source.Query(query, pageNumber, limit, ct);
                return (SourceKey: sourceKey, Result: result);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while querying media source '{SourceKey}' with query '{Query}'.",
                    sourceKey,
                    query);

                return (SourceKey: sourceKey, Result: new PagedList<MediaMetaData>());
            }
        });

        var results = await Task.WhenAll(searchTasks);

        return results.ToDictionary(
            r => r.SourceKey,
            r => r.Result,
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Executes a keyword search query against a specific media source identified by its <see cref="IMediaSource.Source"/> property.
    /// </summary>
    /// <param name="sourceKey">The unique string key of the target media source.</param>
    /// <param name="query">The search term or keyword to query.</param>
    /// <param name="pageNumber">The 1-based page index for pagination.</param>
    /// <param name="limit">The maximum number of items per page.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>The paged search results from the requested media source.</returns>
    public async Task<PagedList<MediaMetaData>> QuerySourceAsync(
        string sourceKey,
        string query,
        int pageNumber,
        int limit,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        if (!_sources.TryGetValue(sourceKey, out var source))
        {
            _logger.LogWarning("Target media source '{SourceKey}' was not found.", sourceKey);
            throw new KeyNotFoundException($"Media source with key '{sourceKey}' is not registered.");
        }

        _logger.LogInformation(
            "Querying individual media source '{SourceKey}' for query '{Query}'.",
            sourceKey,
            query);

        return await source.Query(query, pageNumber, limit, ct);
    }
}
