
// File: LibraryResolver.cs
namespace Viox.Core.Services;

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class LibraryResolver
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LibraryResolver> _logger;
    private readonly IEnumerable<ILibrary> _libraries;

    public LibraryResolver(
        IServiceProvider serviceProvider,
        ILogger<LibraryResolver> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _libraries = serviceProvider.GetKeyedServices<ILibrary>(KeyedService.AnyKey);

        int count = _libraries.Count();
        if (count == 0)
        {
            _logger.LogWarning("Found no ILibrary implementations.");
        }
        else
        {
            _logger.LogInformation("Found {Count} ILibrary implementations.", count);
        }
    }

    /// <summary>
    /// Fetches the registered <see cref="ILibrary"/> by matching its Source using LINQ.
    /// </summary>
    /// <param name="name">The name identifier of the converter to resolve.</param>
    /// <returns>The resolved converter instance, or <c>null</c> if not found.</returns>
    public ILibrary? ResolveLibrary(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        _logger.LogDebug("Searching for converter with Source '{Name}'.", name);

        ILibrary? match = _libraries.FirstOrDefault(c =>
            string.Equals(c.Source, name, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            _logger.LogWarning("No media metadata converter found with Name '{Name}'.", name);
        }

        return match;
    }

}
