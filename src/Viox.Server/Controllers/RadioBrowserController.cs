using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Viox.Client.RadioBrowser.Models;
using Viox.Client.RadioBrowser.Services;
using Viox.Core.Models;
using Viox.Core.Services;

namespace Viox.Server.Controllers;

/// <summary>
/// Options configuration for controlling behavior in <see cref="RadioBrowserController"/>.
/// </summary>
public class RadioBrowserControllerOptions
{
    /// <summary>
    /// Gets or sets the maximum allowed page size for paginated requests.
    /// </summary>
    public int MaxPageLimit { get; set; } = 100;
}

/// <summary>
/// Provides REST API endpoints for searching internet radio stations, retrieving metadata, and casting votes.
/// </summary>
[ApiController]
[Route("api/v1/media/radiobrowser")]
[Tags("Radio Browser")]
[Produces("application/json")]
public class RadioBrowserController : ControllerBase
{
    private readonly IRadioBrowserClient _radioBrowserClient;
    private readonly RadioBrowserControllerOptions _options;
    private readonly ILogger<RadioBrowserController> _logger;
    private readonly MediaMetaDataConverterResolver _resolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="RadioBrowserController"/> class.
    /// </summary>
    /// <param name="radioBrowserClient">The Radio Browser API client service instance.</param>
    /// <param name="options">Options for configuring controller logic.</param>
    /// <param name="logger">The application logging context.</param>
    public RadioBrowserController(
        IRadioBrowserClient radioBrowserClient,
        MediaMetaDataConverterResolver resolver,
        IOptions<RadioBrowserControllerOptions> options,
        ILogger<RadioBrowserController> logger)
    {
        _radioBrowserClient = radioBrowserClient ?? throw new ArgumentNullException(nameof(radioBrowserClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets live statistics for the connected Radio Browser mirror server.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Live statistics for the server.</returns>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ServerStats>> GetStats(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving Radio Browser server stats");

        var stats = await _radioBrowserClient.GetStatsAsync(cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Gets a list of available Radio Browser server mirrors.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A collection of API servers.</returns>
    [HttpGet("servers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ApiServer>>> GetServers(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving Radio Browser servers list");

        var servers = await _radioBrowserClient.GetServersAsync(cancellationToken);
        return Ok(servers);
    }

    /// <summary>
    /// Gets a single radio station by its unique UUID.
    /// </summary>
    /// <param name="stationUuid">The unique identifier of the station.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The matching radio station if found.</returns>
    [HttpGet("stations/{stationUuid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MediaMetaData>> GetStationByUuid(
        string stationUuid,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving station with UUID {StationUuid}", stationUuid);
        MediaUri? uri = MediaUriParser.ParseMediaUriValue(stationUuid);

        var station = await _radioBrowserClient.GetStationByUuidAsync(uri is null ? stationUuid : uri.Id, cancellationToken);
        if (station is null)
        {
            _logger.LogWarning("Station with UUID {StationUuid} was not found", stationUuid);
            return NotFound();
        }

        return Ok(_resolver.Convert(station));
    }

    /// <summary>
    /// Gets the top clicked stations.
    /// </summary>
    [HttpGet("stations/top-clicked")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IList<MediaMetaData>>> GetTopClickedStations(
        [FromQuery] ListQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving top-clicked stations");

        var stations = await _radioBrowserClient.GetTopClickedStationsAsync(options, cancellationToken);
        return Ok(_resolver.ConvertList(stations));
    }

    /// <summary>
    /// Gets the top voted stations.
    /// </summary>
    [HttpGet("stations/top-voted")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IList<MediaMetaData>>> GetTopVotedStations(
        [FromQuery] ListQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving top-voted stations");

        var stations = await _radioBrowserClient.GetTopVotedStationsAsync(options, cancellationToken);
        return Ok(_resolver.ConvertList(stations));
    }

    /// <summary>
    /// Gets stations that were clicked most recently.
    /// </summary>
    [HttpGet("stations/recently-clicked")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IList<MediaMetaData>>> GetRecentlyClickedStations(
        [FromQuery] ListQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving recently-clicked stations");

        var stations = await _radioBrowserClient.GetRecentlyClickedStationsAsync(options, cancellationToken);
        return Ok(_resolver.ConvertList(stations));
    }

    /// <summary>
    /// Records a play/click event for a target station and resolves its stream URL.
    /// </summary>
    /// <param name="stationUuid">The unique identifier of the station being clicked.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Click outcome including resolved stream details.</returns>
    [HttpPost("stations/{stationUuid}/click")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ClickResult>> ClickStation(
        string stationUuid,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recording play click event for station UUID {StationUuid}", stationUuid);
        MediaUri? uri = MediaUriParser.ParseMediaUriValue(stationUuid);

        var result = await _radioBrowserClient.ClickStationAsync(uri is null ? stationUuid : uri.Id, cancellationToken);
        return Ok(result);
    }

}
