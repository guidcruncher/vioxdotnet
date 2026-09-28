namespace Viox.Server.Controllers;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Viox.Core.Models;
using Viox.Core.Services;

/// <summary>
/// Handles HTTP requests for searching media metadata across registered media sources.
/// </summary>
[ApiController]
[Route("api/v1/media/search")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Media Search")]
public class MediaSearchController : ControllerBase
{
    private readonly MediaSearchService _searchService;
    private readonly ILogger<MediaSearchController> _logger;

    public MediaSearchController(
        MediaSearchService searchService,
        ILogger<MediaSearchController> logger)
    {
        ArgumentNullException.ThrowIfNull(searchService);
        ArgumentNullException.ThrowIfNull(logger);

        _searchService = searchService;
        _logger = logger;
    }

    /// <summary>
    /// Broadcasts a search query across all available media sources concurrently.
    /// </summary>
    /// <param name="query">The search string or keyword.</param>
    /// <param name="pageNumber">The 1-based page index for pagination.</param>
    /// <param name="limit">The maximum number of items returned per page per source.</param>
    /// <param name="ct">Cancellation token for cancelling the asynchronous request.</param>
    /// <returns>A dictionary containing search results keyed by media source identifier.</returns>
    [HttpGet]
    [EndpointSummary("Search all media sources")]
    [EndpointDescription("Executes a keyword query concurrently across every registered media source and returns aggregated results.")]
    [ProducesResponseType(typeof(IReadOnlyDictionary<string, PagedList<MediaMetaData>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyDictionary<string, PagedList<MediaMetaData>>>> QueryAllSources(
        [FromQuery, Required] string query,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        if (pageNumber < 1)
        {
            ModelState.AddModelError(nameof(pageNumber), "Page number must be greater than or equal to 1.");
            return ValidationProblem(ModelState);
        }

        if (limit < 1)
        {
            ModelState.AddModelError(nameof(limit), "Limit must be greater than or equal to 1.");
            return ValidationProblem(ModelState);
        }

        _logger.LogInformation(
            "Received HTTP request to search all media sources for query '{Query}' (Page: {PageNumber}, Limit: {Limit}).",
            query,
            pageNumber,
            limit);

        var results = await _searchService.QueryAllSourcesAsync(query, pageNumber, limit, ct);

        return Ok(results);
    }

    /// <summary>
    /// Searches a specific media source by its source key.
    /// </summary>
    /// <param name="sourceKey">The unique identifier string of the media source (e.g., 'librespot', 'youtube').</param>
    /// <param name="query">The search string or keyword.</param>
    /// <param name="pageNumber">The 1-based page index for pagination.</param>
    /// <param name="limit">The maximum number of items returned per page.</param>
    /// <param name="ct">Cancellation token for cancelling the asynchronous request.</param>
    /// <returns>A paged list of matching media metadata from the target source.</returns>
    [HttpGet("{sourceKey}")]
    [EndpointSummary("Search a specific media source")]
    [EndpointDescription("Executes a keyword search query against a single specified media source identified by its source key.")]
    [ProducesResponseType(typeof(PagedList<MediaMetaData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedList<MediaMetaData>>> QuerySource(
        [FromRoute] string sourceKey,
        [FromQuery, Required] string query,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sourceKey))
        {
            ModelState.AddModelError(nameof(sourceKey), "Source key cannot be empty.");
            return ValidationProblem(ModelState);
        }

        if (pageNumber < 1)
        {
            ModelState.AddModelError(nameof(pageNumber), "Page number must be greater than or equal to 1.");
            return ValidationProblem(ModelState);
        }

        if (limit < 1)
        {
            ModelState.AddModelError(nameof(limit), "Limit must be greater than or equal to 1.");
            return ValidationProblem(ModelState);
        }

        _logger.LogInformation(
            "Received HTTP request to search media source '{SourceKey}' for query '{Query}' (Page: {PageNumber}, Limit: {Limit}).",
            sourceKey,
            query,
            pageNumber,
            limit);

        try
        {
            var results = await _searchService.QuerySourceAsync(sourceKey, query, pageNumber, limit, ct);
            return Ok(results);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Media source '{SourceKey}' was not found.", sourceKey);

            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Media Source Not Found",
                detail: ex.Message);
        }
    }
}
