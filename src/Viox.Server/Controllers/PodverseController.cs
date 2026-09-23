namespace Viox.Server.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Viox.Client.Podverse.Models;
using Viox.Client.Podverse.Services;
using Viox.Core.Models;
using Viox.Core.Services;

/// <summary>
/// Web API Controller exposing Podverse client functionality via RESTful HTTP endpoints.
/// </summary>
/// <param name="podverseClient">The injected Podverse API client service.</param>
/// <param name="resolver">Media MetaData Converter Resolver.</param>
/// <param name="rssParser">RSS Parser.</param>
/// <param name="logger">The injected logger instance.</param>
[ApiController]
[Route("api/v1/media/podverse")]
[Tags("Podverse")]
[Produces("application/json")]
public class PodverseController(
    IPodverseClient podverseClient,
    MediaMetaDataConverterResolver resolver,
    PodcastEpisodeParser rssParser,
    ILogger<PodverseController> logger) : ControllerBase
{
    private readonly IPodverseClient _podverseClient = podverseClient ?? throw new ArgumentNullException(nameof(podverseClient));
    private readonly ILogger<PodverseController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly PodcastEpisodeParser _rssParser = rssParser ?? throw new ArgumentNullException(nameof(rssParser));
    private readonly MediaMetaDataConverterResolver _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));

    /// <summary>
    /// Retrieves a list of podcast episodes by podcast identifier.
    /// </summary>
    /// <param name="podcastId">The unique ID of the podcast.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The podcast instance if found; otherwise, 404 Not Found.</returns>
    [HttpGet("podcast/{podcastId}/episodes")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MediaMetaData>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<MediaMetaData>>> GetPodcastEpisodesById(
        [FromRoute] string podcastId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(podcastId))
        {
            return BadRequest("Podcast ID must be specified.");
        }

        _logger.LogInformation("Fetching podcast by ID: {PodcastId}", podcastId);

        IMediaMetaDataConverterBase? converter = _resolver.ResolveConverter("podverse:episode");
        if (converter is null)
        {
            return BadRequest("Could not load converter");
        }

        MediaUri? uri = MediaUriParser.ParseMediaUriValue(podcastId);

        var podcast = await _podverseClient.GetPodcastByIdAsync((uri is null ? podcastId : uri.Id), cancellationToken);
        if (podcast is null)
        {
            _logger.LogWarning("Podcast with ID {PodcastId} was not found.", podcastId);
            return NotFound();
        }

        if (podcast.FeedUrls?.Count > 0)
        {
            FeedUrl? firstPublicFeed = podcast.FeedUrls.FirstOrDefault(f => f.IsPublic is true)
                ?? podcast.FeedUrls.FirstOrDefault(f => !string.IsNullOrEmpty(f.Url));

            if (firstPublicFeed != null)
            {
                IReadOnlyList<PodcastEpisode> episodes = await _rssParser.ParseEpisodesFromUrlAsync(podcast.Id, firstPublicFeed.Url, cancellationToken);
                List<MediaMetaData> items = [];

                foreach (PodcastEpisode episode in episodes)
                {
                    MediaMetaData? item = converter.Convert(episode);
                    if (item is not null)
                    {
                        if (string.IsNullOrEmpty(item.ImageUrl))
                        {
                            item.ImageUrl = !string.IsNullOrEmpty(podcast.ImageUrl) ? podcast.ImageUrl : string.Empty;
                        }

                        items.Add(item);
                    }
                }

                return Ok(items);
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MediaMetaData))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MediaMetaData>> GetPodcastById(
        [FromRoute] string podcastId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(podcastId))
        {
            return BadRequest("Podcast ID must be specified.");
        }

        _logger.LogInformation("Fetching podcast by ID: {PodcastId}", podcastId);

        MediaUri? uri = MediaUriParser.ParseMediaUriValue(podcastId);

        var podcast = await _podverseClient.GetPodcastByIdAsync((uri is null ? podcastId : uri.Id), cancellationToken);
        if (podcast is null)
        {
            _logger.LogWarning("Podcast with ID {PodcastId} was not found.", podcastId);
            return NotFound();
        }


        return Ok(_resolver.Convert(podcast));
    }

    /// <summary>
    /// Retrieves an episode by its unique identifier.
    /// </summary>
    /// <param name="episodeId">The unique ID of the episode.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The episode instance if found; otherwise, 404 Not Found.</returns>
    [HttpGet("episodes/{episodeId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MediaMetaData))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MediaMetaData>> GetEpisodeById(
        [FromRoute] string episodeId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(episodeId))
        {
            return BadRequest("Episode ID must be specified.");
        }

        MediaUri? uri = MediaUriParser.ParseMediaUriValue(episodeId);
        if (uri is null)
        {
            return BadRequest("Episode ID must be specified.");
        }

        _logger.LogInformation("Fetching podcast by ID: {PodcastId}", uri.Id);
        IMediaMetaDataConverterBase? converter = _resolver.ResolveConverter("podverse:episode");

        if (converter is null)
        {
            return BadRequest("Could not load converter");
        }

        var podcast = await _podverseClient.GetPodcastByIdAsync((uri is null ? episodeId : uri.Id), cancellationToken);
        if (podcast is null)
        {
            _logger.LogWarning("Podcast with ID {PodcastId} was not found.", episodeId);
            return NotFound();
        }
        if (podcast.FeedUrls?.Count > 0)
        {
            FeedUrl? firstPublicFeed = podcast.FeedUrls.FirstOrDefault(f => f.IsPublic is true)
                ?? podcast.FeedUrls.FirstOrDefault(f => !string.IsNullOrEmpty(f.Url));
            if (firstPublicFeed != null)
            {
                IReadOnlyList<PodcastEpisode> episodes = await _rssParser.ParseEpisodesFromUrlAsync(podcast.Id, firstPublicFeed.Url, cancellationToken);

                foreach (PodcastEpisode episode in episodes)
                {
                    MediaMetaData? item = converter.Convert(episode);
                    if (item is not null)
                    {
                        if (item.RawUri == episodeId)
                        {
                            return Ok(item);
                        }
                    }
                }
            }
        }
        return NotFound();
    }
}
