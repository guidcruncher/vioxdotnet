using Viox.Client.Podverse.Models;

namespace Viox.Client.Podverse.Services;

/// <summary>
/// Client contract for interacting with the Podverse API endpoints.
/// </summary>
public interface IPodverseClient
{
    /// <summary>
    /// Retrieves a podcast by its unique identifier.
    /// </summary>
    /// <param name="podcastId">The ID of the podcast to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The podcast instance if found; otherwise, <c>null</c>.</returns>
    Task<Podcast?> GetPodcastByIdAsync(string podcastId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches podcasts using query parameters and returns matching podcast records.
    /// </summary>
    /// <param name="searchTitle">Optional title string to filter podcasts.</param>
    /// <param name="page">Optional page index for pagination.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A collection of matching podcast records.</returns>
    Task<IReadOnlyList<Podcast>> GetPodcastsAsync(string? searchTitle = null, int? page = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches podcasts using query parameters and returns a paginated result containing items and total record count.
    /// </summary>
    /// <param name="searchTitle">Optional title string to filter podcasts.</param>
    /// <param name="page">Optional page index for pagination.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A paginated result containing podcasts and total count metadata.</returns>
    Task<PodversePagedResult<Podcast>?> GetPodcastsPagedAsync(string? searchTitle = null, int? page = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an episode by its unique identifier.
    /// </summary>
    /// <param name="episodeId">The ID of the episode to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The episode instance if found; otherwise, <c>null</c>.</returns>
    Task<Episode?> GetEpisodeByIdAsync(string episodeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets information for the currently authenticated user.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The authenticated user information if authorized; otherwise, <c>null</c>.</returns>
    Task<User?> GetAuthenticatedUserInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Toggles the subscription state for a given podcast.
    /// </summary>
    /// <param name="podcastId">The ID of the podcast to toggle subscription for.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if the request completed successfully; otherwise, <c>false</c>.</returns>
    Task<bool> TogglePodcastSubscriptionAsync(string podcastId, CancellationToken cancellationToken = default);
}
