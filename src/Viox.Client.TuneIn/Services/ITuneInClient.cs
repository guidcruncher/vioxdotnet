using Viox.Client.TuneIn.Models;

namespace Viox.Client.TuneIn.Services;

/// <summary>
/// Client interface for interacting with the TuneIn Streaming API.
/// </summary>
public interface ITuneInClient
{
    /// <summary>
    /// Browses through the TuneIn directory or categories via Browse.ashx.
    /// </summary>
    /// <param name="category">Optional category parameter (e.g., "local", "music", "talk", "sports").</param>
    /// <param name="id">Optional category or location identifier (e.g., "r0", "g61").</param>
    /// <param name="filter">Optional filter value.</param>
    /// <param name="cancellationToken">Cancellation token for cancelling the operation.</param>
    /// <returns>A task representing the asynchronous operation, returning the <see cref="TuneInResponse"/>.</returns>
    Task<TuneInResponse<TuneInOutline>?> BrowseAsync(string? category = null, string? id = null, string? filter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves information about a specific station, show, topic, or podcast via Describe.ashx.
    /// </summary>
    /// <param name="id">The unique guide ID or entity ID to describe.</param>
    /// <param name="cancellationToken">Cancellation token for cancelling the operation.</param>
    /// <returns>A task representing the asynchronous operation, returning the <see cref="TuneInResponse"/>.</returns>
    Task<TuneInResponse<StationElement>?> DescribeAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for stations, shows, topics, and podcasts via Search.ashx.
    /// </summary>
    /// <param name="query">The search term query string.</param>
    /// <param name="cancellationToken">Cancellation token for cancelling the operation.</param>
    /// <returns>A task representing the asynchronous operation, returning the <see cref="TuneInResponse"/>.</returns>
    Task<TuneInResponse<TuneInOutline>?> SearchAsync(string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches play stream links for a target station via Tune.ashx.
    /// </summary>
    /// <param name="id">The station ID (e.g., "s12345").</param>
    /// <param name="filter">Optional stream filter parameters.</param>
    /// <param name="cancellationToken">Cancellation token for cancelling the operation.</param>
    /// <returns>A task representing the asynchronous operation, returning the <see cref="TuneInResponse"/>.</returns>
    Task<TuneInResponse<AudioElement>?> TuneAsync(string id, string? filter = null, CancellationToken cancellationToken = default);
}
