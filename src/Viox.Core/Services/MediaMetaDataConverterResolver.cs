
// File: MediaMetaDataConverterResolver.cs
namespace Viox.Core.Services;

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Viox.Core.Models;

/// <summary>
/// Dynamically resolves <see cref="IMediaMetaDataConverterBase"/> instances from registered services.
/// </summary>
public class MediaMetaDataConverterResolver
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MediaMetaDataConverterResolver> _logger;
    private readonly IEnumerable<IMediaMetaDataConverterBase> _converters;

    public MediaMetaDataConverterResolver(
        IServiceProvider serviceProvider,
        ILogger<MediaMetaDataConverterResolver> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _converters = serviceProvider.GetKeyedServices<IMediaMetaDataConverterBase>(KeyedService.AnyKey);

        int count = _converters.Count();
        if (count == 0)
        {
            _logger.LogWarning("Found no IMediaMetaDataConverterBase implementations.");
        }
        else
        {
            _logger.LogInformation("Found {Count} IMediaMetaDataConverterBase implementations.", count);
        }
    }

    /// <summary>
    /// Finds a registered converter matching the specified source and type.
    /// </summary>
    /// <param name="source">The source origin identifier.</param>
    /// <param name="type">The media target type.</param>
    /// <returns>The matching converter instance, or <c>null</c> if not found.</returns>
    public IMediaMetaDataConverterBase? FindConverter(string source, string type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(type);

        _logger.LogDebug("Searching for converter with Source '{Source}' and Type '{Type}'.", source, type);

        IMediaMetaDataConverterBase? match = _converters.FirstOrDefault(c =>
            string.Equals(c.Source, source, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(c.Type, type, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            _logger.LogWarning("No media metadata converter found for Source '{Source}' and Type '{Type}'.", source, type);
        }

        return match;
    }

    /// <summary>
    /// Converts a collection of raw source objects into media metadata objects.
    /// </summary>
    /// <param name="sourceArray">The collection of dynamic input objects.</param>
    /// <returns>A collection of converted media metadata objects.</returns>
    public IEnumerable<MediaMetaData> ConvertList(IEnumerable<dynamic> sourceArray)
    {
        ArgumentNullException.ThrowIfNull(sourceArray);

        List<MediaMetaData> results = new();

        foreach (dynamic source in sourceArray)
        {
            if (source is null)
            {
                continue;
            }

            MediaMetaData? item = Convert(source);
            if (item is not null)
            {
                results.Add(item);
            }
        }

        return results;
    }

    /// <summary>
    /// Converts a single raw source object into media metadata.
    /// </summary>
    /// <param name="source">The dynamic input object containing media parameters.</param>
    /// <returns>The converted metadata instance, or <c>null</c> if conversion fails.</returns>
    public MediaMetaData? Convert(dynamic source)
    {
        if (source is null)
        {
            _logger.LogError("Source is null");
            return null;
        }

        string? uriString;
        try
        {
            uriString = System.Convert.ToString((object?)source.Uri);
        }
        catch (Exception ex)
        {
            _logger.LogError("Cannot extract URI {ex}", ex);
            return null;
        }

        if (string.IsNullOrWhiteSpace(uriString))
        {
            _logger.LogError("URI is empty");
            return null;
        }

        MediaUri? uri = MediaUriParser.ParseMediaUriValue(uriString);

        if (uri is null)
        {
            return null;
        }

        IMediaMetaDataConverterBase? match = FindConverter(uri.Source, uri.Type);

        return match?.Convert(source);
    }

    /// <summary>
    /// Fetches the registered <see cref="IMediaMetaDataConverterBase"/> by matching its name using LINQ.
    /// </summary>
    /// <param name="name">The name identifier of the converter to resolve.</param>
    /// <returns>The resolved converter instance, or <c>null</c> if not found.</returns>
    public IMediaMetaDataConverterBase? ResolveConverter(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _logger.LogDebug("Searching for converter with Name '{Name}'.", name);

        IMediaMetaDataConverterBase? match = _converters.FirstOrDefault(c =>
            string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            _logger.LogWarning("No media metadata converter found with Name '{Name}'.", name);
        }

        return match;
    }

    /// <summary>
    /// Fetches the registered <see cref="IMediaMetaDataConverterBase"/> using a <see cref="MediaUri"/>.
    /// </summary>
    /// <param name="uri">The media URI containing source and type details.</param>
    /// <returns>The resolved converter instance, or <c>null</c> if not found.</returns>
    public IMediaMetaDataConverterBase? ResolveConverter(MediaUri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);

        string name = $"{uri.Source}:{uri.Type}";
        return ResolveConverter(name);
    }
}
