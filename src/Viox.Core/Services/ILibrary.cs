// File: ILibrary.cs
namespace Viox.Core.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Viox.Core.Models;

/// <summary>
/// Contract for media library data access services.
/// </summary>
public interface ILibrary
{
    /// <summary>
    /// Gets the implementation-specific static source identifier.
    /// </summary>
    string Source { get; }

    /// <summary>
    /// Asynchronously reads metadata entries from the media library.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A collection of media metadata items.</returns>
    Task<IList<MediaMetaData>> ReadAsync(Dictionary<string, object>? parameters, CancellationToken cancellationToken = default);
}
