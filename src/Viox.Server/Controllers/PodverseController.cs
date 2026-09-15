using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Viox.Client.Podverse.Models;
using Viox.Client.Podverse.Services;
using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Server.Controllers;

/// <summary>
/// Web API Controller exposing Podverse client functionality via RESTful HTTP endpoints.
/// </summary>
/// <param name="podverseClient">The injected Podverse API client service.</param>
/// <param name="rssParser">RSS Parser</param>
/// <param name="logger">The injected logger instance.</param>
[ApiController]
[Route("api/v1/media/podverse")]
[Tags("Podverse")]
[Produces("application/json")]
public class PodverseController(
    IPodverseClient podverseClient,
    PodcastEpisodeParser rssParser,
    ILogger<PodverseController> logger) : ControllerBase
{
    private readonly IPodverseClient _podverseClient = podverseClient ?? throw new ArgumentNullException(nameof(podverseClient));
    private readonly ILogger<PodverseController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly PodcastEpisodeParser _rssParser = rssParser ?? throw new ArgumentNullException(nameof(rssParser));

    /// <summary>
    /// Retrieves a list of podcast episodes by podcast identifier.
    /// </summary>
    /// <param name="podcastId">The unique ID of the podcast.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The podcast instance if found; otherwise, 404 Not Found.</returns>
    [HttpGet("podcast/{podcastId}/episodes")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyList<PodcastEpisode>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PodcastEpisode>>> GetPodcastEpisodesById(
        [FromRoute] string podcastId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(podcastId))
        {
            return BadRequest("Podcast ID must be specified.");
        }

        _logger.LogInformation("Fetching podcast by ID: {PodcastId}", podcastId);

        var podcast = await _podverseClient.GetPodcastByIdAsync(podcastId, cancellationToken);
        if (podcast is null)
        {
            _logger.LogWarning("Podcast with ID {PodcastId} was not found.", podcastId);
            return NotFound();
        }

        if (podcast.FeedUrls?.Count > 0)
        {
            FeedUrl? firstPublicFeed = podcast.FeedUrls.FirstOrDefault(f => f.IsPublic is true);
            if (firstPublicFeed == null)
            {
                firstPublicFeed = podcast.FeedUrls.FirstOrDefault(f => !string.IsNullOrEmpty(f.Url));
            }

            if (firstPublicFeed != null)
            {
                IReadOnlyList<PodcastEpisode> episodes = await _rssParser.ParseEpisodesFromUrlAsync(podcast.Id, firstPublicFeed.Url, cancellationToken);
                return Ok(episodes);
            }
        }

        return NotFound();
    }

    /// <summary>
    /// Retrieves a podcast by its unique identifier.
    /// </summary>
    /// <param name="podcastId">The unique ID of the podcast.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The podcast instance if found; otherwise, 404 Not Found.</returns>
    [HttpGet("podcasts/{podcastId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Podcast))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Podcast>> GetPodcastById(
        [FromRoute] string podcastId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(podcastId))
        {
            return BadRequest("Podcast ID must be specified.");
        }

        _logger.LogInformation("Fetching podcast by ID: {PodcastId}", podcastId);

        var podcast = await _podverseClient.GetPodcastByIdAsync(podcastId, cancellationToken);
        if (podcast is null)
        {
            _logger.LogWarning("Podcast with ID {PodcastId} was not found.", podcastId);
            return NotFound();
        }

        return Ok(podcast);
    }

    /// <summary>
    /// Searches or lists podcasts using optional query criteria.
    /// </summary>
    /// <param name="searchTitle">Optional title filter.</param>
    /// <param name="page">Optional pagination page index.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A collection of matching podcasts.</returns>
    [HttpGet("podcasts")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IReadOnlyList<Podcast>))]
    public async Task<ActionResult<IReadOnlyList<Podcast>>> GetPodcasts(
        [FromQuery] string? searchTitle,
        [FromQuery] int? page,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching podcasts with SearchTitle: '{SearchTitle}', Page: {Page}", searchTitle, page);

        var podcasts = await _podverseClient.GetPodcastsAsync(searchTitle, page, cancellationToken);
        return Ok(podcasts);
    }

    /// <summary>
    /// Retrieves an episode by its unique identifier.
    /// </summary>
    /// <param name="episodeId">The unique ID of the episode.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The episode instance if found; otherwise, 404 Not Found.</returns>
    [HttpGet("episodes/{episodeId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Episode))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Episode>> GetEpisodeById(
        [FromRoute] string episodeId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(episodeId))
        {
            return BadRequest("Episode ID must be specified.");
        }

        _logger.LogInformation("Fetching episode by ID: {EpisodeId}", episodeId);

        var episode = await _podverseClient.GetEpisodeByIdAsync(episodeId, cancellationToken);
        if (episode is null)
        {
            _logger.LogWarning("Episode with ID {EpisodeId} was not found.", episodeId);
            return NotFound();
        }

        return Ok(episode);
    }

    /// <summary>
    /// Gets information for the currently authenticated user.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The user profile if authorized; otherwise, 401 Unauthorized.</returns>
    [HttpGet("user/me")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<User>> GetAuthenticatedUserInfo(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving authenticated user info.");

        var user = await _podverseClient.GetAuthenticatedUserInfoAsync(cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Unauthorized attempt to retrieve user info.");
            return Unauthorized();
        }

        return Ok(user);
    }

    /// <summary>
    /// Toggles the subscription status for a given podcast.
    /// </summary>
    /// <param name="podcastId">The ID of the podcast to toggle subscription for.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>200 OK if subscription toggled successfully; otherwise, 400 Bad Request.</returns>
    [HttpPost("podcasts/{podcastId}/toggle-subscription")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TogglePodcastSubscription(
        [FromRoute] string podcastId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(podcastId))
        {
            return BadRequest("Podcast ID must be specified.");
        }

        _logger.LogInformation("Toggling subscription for podcast ID: {PodcastId}", podcastId);

        var success = await _podverseClient.TogglePodcastSubscriptionAsync(podcastId, cancellationToken);
        if (!success)
        {
            _logger.LogWarning("Failed to toggle subscription for podcast ID: {PodcastId}", podcastId);
            return BadRequest("Unable to toggle podcast subscription.");
        }

        return Ok();
    }
}
