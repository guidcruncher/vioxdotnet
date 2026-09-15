using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Client.TuneIn.Models;
using Viox.Client.TuneIn.Services;

namespace Viox.Server.Controllers;

/// <summary>
/// Options configuration for controlling behavior in <see cref="TuneInController"/>.
/// </summary>
public class TuneInControllerOptions
{
    /// <summary>
    /// Gets or sets the default category used when browsing if none is specified.
    /// </summary>
    public string? DefaultCategory { get; set; }

    /// <summary>
    /// Gets or sets the default filter used for stream tuning if none is specified.
    /// </summary>
    public string? DefaultTuneFilter { get; set; }
}

/// <summary>
/// Provides REST API endpoints for browsing, describing, searching, and tuning streams via the TuneIn API.
/// </summary>
[ApiController]
[Route("api/v1/media/tunein")]
[Tags("TuneIn")]
[Produces("application/json")]
public class TuneInController : ControllerBase
{
    private readonly ITuneInClient _tuneInClient;
    private readonly TuneInControllerOptions _options;
    private readonly ILogger<TuneInController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TuneInController"/> class.
    /// </summary>
    /// <param name="tuneInClient">The TuneIn client service instance.</param>
    /// <param name="options">Options for configuring controller logic.</param>
    /// <param name="logger">The application logging context.</param>
    public TuneInController(
        ITuneInClient tuneInClient,
        IOptions<TuneInControllerOptions> options,
        ILogger<TuneInController> logger)
    {
        _tuneInClient = tuneInClient ?? throw new ArgumentNullException(nameof(tuneInClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Browses through the TuneIn directory or categories.
    /// </summary>
    /// <param name="category">Optional category parameter (e.g., "local", "music", "talk", "sports").</param>
    /// <param name="id">Optional category or location identifier (e.g., "r0", "g61").</param>
    /// <param name="filter">Optional filter value.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Directory elements matching the requested path.</returns>
    [HttpGet("browse")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TuneInResponse<TuneInOutline>>> Browse(
        [FromQuery] string? category = null,
        [FromQuery] string? id = null,
        [FromQuery] string? filter = null,
        CancellationToken cancellationToken = default)
    {
        var targetCategory = category ?? _options.DefaultCategory;
        _logger.LogInformation("Browsing TuneIn directory. Category: {Category}, Id: {Id}, Filter: {Filter}", targetCategory, id, filter);

        var result = await _tuneInClient.BrowseAsync(targetCategory, id, filter, cancellationToken);
        if (result is null)
        {
            _logger.LogWarning("Browse request returned no data. Category: {Category}, Id: {Id}", targetCategory, id);
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Retrieves detailed metadata about a specific station, show, topic, or podcast.
    /// </summary>
    /// <param name="id">The unique guide ID or entity ID to describe.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Metadata describing the entity if found.</returns>
    [HttpGet("describe/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TuneInResponse<StationElement>>> Describe(
        string id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving details for TuneIn entity {EntityId}", id);

        var result = await _tuneInClient.DescribeAsync(id, cancellationToken);
        if (result is null)
        {
            _logger.LogWarning("Entity {EntityId} not found in TuneIn", id);
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Searches for stations, shows, topics, and podcasts matching a query string.
    /// </summary>
    /// <param name="query">The search term query string.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Search results matching the search term.</returns>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TuneInOutline>> Search(
        [FromQuery, Required] string query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching TuneIn with query '{Query}'", query);

        var result = await _tuneInClient.SearchAsync(query, cancellationToken);
        if (result is null)
        {
            _logger.LogWarning("No search results returned for query '{Query}'", query);
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Fetches play stream links for a target station.
    /// </summary>
    /// <param name="id">The station ID (e.g., "s12345").</param>
    /// <param name="filter">Optional stream filter parameters.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Play stream links for the specified station.</returns>
    [HttpGet("tune/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TuneInResponse<AudioElement>>> Tune(
        string id,
        [FromQuery] string? filter = null,
        CancellationToken cancellationToken = default)
    {
        var targetFilter = filter ?? _options.DefaultTuneFilter;
        _logger.LogInformation("Retrieving stream links for station {StationId} with filter '{Filter}'", id, targetFilter);

        var result = await _tuneInClient.TuneAsync(id, targetFilter, cancellationToken);
        if (result is null)
        {
            _logger.LogWarning("Stream links for station {StationId} could not be resolved", id);
            return NotFound();
        }

        return Ok(result);
    }
}
