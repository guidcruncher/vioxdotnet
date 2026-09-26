// File: IMediaSource.cs
namespace Viox.Core.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Viox.Core.Models;

/// <summary>
/// Defines a contract for retrieving metadata for media items identified by a <see cref="MediaUri"/>.
/// </summary>
public interface IMediaSource
{
    string Source { get; }

    /// <summary>
    /// Asynchronously resolves metadata details for the specified media URI.
    /// </summary>
    /// <param name="uri">The structured <see cref="MediaUri"/> identifying the resource to resolve.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task representing the asynchronous operation that yields the resolved <see cref="MediaMetaData"/>.
    /// </returns>
    Task<MediaMetaData?> ResolveMetaData(MediaUri? uri, CancellationToken ct);

    Task<IList<MediaMetaData>> ResolveChildItems(MediaUri? parent, string childType, CancellationToken ct);

    Task<PagedList<MediaMetaData>> Query(string query, int pageNumber, int limit, CancellationToken ct = default);

    /// <summary>
    /// Asynchronously reads metadata entries from the media library.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A collection of media metadata items.</returns>
    Task<IList<MediaMetaData>> ReadAsync(Dictionary<string, object>? parameters, CancellationToken cancellationToken = default);
}
