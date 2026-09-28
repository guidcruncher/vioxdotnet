namespace Viox.Server.Services;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Viox.Core.Models;

/// <summary>
/// Abstraction for providing target podcast media metadata to background jobs.
/// </summary>
public interface IPodcastProvider
{
    /// <summary>
    /// Retrieves the collection of podcast media items scheduled for downloading.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of media metadata items.</returns>
    Task<IEnumerable<MediaMetaData>> GetPodcastsToDownloadAsync(CancellationToken cancellationToken);
}
