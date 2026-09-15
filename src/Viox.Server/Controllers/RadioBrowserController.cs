using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Viox.Client.RadioBrowser.Models;
using Viox.Client.RadioBrowser.Services;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="RadioBrowserController"/> class.
    /// </summary>
    /// <param name="radioBrowserClient">The Radio Browser API client service instance.</param>
    /// <param name="options">Options for configuring controller logic.</param>
    /// <param name="logger">The application logging context.</param>
    public RadioBrowserController(
        IRadioBrowserClient radioBrowserClient,
        IOptions<RadioBrowserControllerOptions> options,
        ILogger<RadioBrowserController> logger)
    {
        _radioBrowserClient = radioBrowserClient ?? throw new ArgumentNullException(nameof(radioBrowserClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
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
    /// Searches radio stations matching complex filter options.
    /// </summary>
    /// <param name="options">Station search filter parameters.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A collection of matching radio stations.</returns>
    [HttpPost("stations/search")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Station>>> SearchStations(
        [FromBody] StationSearchOptions? options,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing advanced station search");

        var stations = await _radioBrowserClient.SearchStationsAsync(options, cancellationToken);
        return Ok(stations);
    }

    /// <summary>
    /// Gets a list of stations using default or general listing options.
    /// </summary>
    /// <param name="options">Query options for paging and sorting.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A collection of radio stations.</returns>
    [HttpGet("stations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Station>>> GetStations(
        [FromQuery] ListQueryOptions? options,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Listing stations with query options");

        var stations = await _radioBrowserClient.GetStationsAsync(options, cancellationToken);
        return Ok(stations);
    }

    /// <summary>
    /// Gets multiple stations matching a list of UUIDs.
    /// </summary>
    /// <param name="uuids">A collection of station UUID strings.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A collection of matching radio stations.</returns>
    [HttpPost("stations/by-uuids")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Station>>> GetStationsByUuid(
        [FromBody] IEnumerable<string> uuids,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving stations for multiple UUIDs");

        var stations = await _radioBrowserClient.GetStationsByUuidAsync(uuids, cancellationToken);
        return Ok(stations);
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
    public async Task<ActionResult<Station>> GetStationByUuid(
        string stationUuid,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving station with UUID {StationUuid}", stationUuid);

        var station = await _radioBrowserClient.GetStationByUuidAsync(stationUuid, cancellationToken);
        if (station is null)
        {
            _logger.LogWarning("Station with UUID {StationUuid} was not found", stationUuid);
            return NotFound();
        }

        return Ok(station);
    }

    /// <summary>
    /// Searches radio stations by name substring or exact match.
    /// </summary>
    /// <param name="name">The name or part of the name to search for.</param>
    /// <param name="exact">Whether to match the exact name string.</param>
    /// <param name="options">Query options for paging and sorting.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A collection of matching radio stations.</returns>
    [HttpGet("stations/by-name")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Station>>> GetStationsByName(
        [FromQuery, Required] string name,
        [FromQuery] bool exact = false,
        [FromQuery] ListQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving stations by name '{Name}' (Exact: {Exact})", name, exact);

        var stations = exact
            ? await _radioBrowserClient.GetStationsByNameExactAsync(name, options, cancellationToken)
            : await _radioBrowserClient.GetStationsByNameAsync(name, options, cancellationToken);

        return Ok(stations);
    }

    /// <summary>
    /// Searches radio stations by tag substring or exact match.
    /// </summary>
    /// <param name="tag">The tag name to search for.</param>
    /// <param name="exact">Whether to match the exact tag string.</param>
    /// <param name="options">Query options for paging and sorting.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A collection of matching radio stations.</returns>
    [HttpGet("stations/by-tag")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Station>>> GetStationsByTag(
        [FromQuery, Required] string tag,
        [FromQuery] bool exact = false,
        [FromQuery] ListQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving stations by tag '{Tag}' (Exact: {Exact})", tag, exact);

        var stations = exact
            ? await _radioBrowserClient.GetStationsByTagExactAsync(tag, options, cancellationToken)
            : await _radioBrowserClient.GetStationsByTagAsync(tag, options, cancellationToken);

        return Ok(stations);
    }

    /// <summary>
    /// Searches radio stations by country name or exact match.
    /// </summary>
    /// <param name="country">The country name.</param>
    /// <param name="exact">Whether to match the exact country string.</param>
    /// <param name="options">Query options for paging and sorting.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A collection of matching radio stations.</returns>
    [HttpGet("stations/by-country")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Station>>> GetStationsByCountry(
        [FromQuery, Required] string country,
        [FromQuery] bool exact = false,
        [FromQuery] ListQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving stations by country '{Country}' (Exact: {Exact})", country, exact);

        var stations = exact
            ? await _radioBrowserClient.GetStationsByCountryExactAsync(country, options, cancellationToken)
            : await _radioBrowserClient.GetStationsByCountryAsync(country, options, cancellationToken);

        return Ok(stations);
    }

    /// <summary>
    /// Searches radio stations by ISO 3166-1 alpha-2 country code.
    /// </summary>
    /// <param name="countryCode">The 2-letter ISO country code.</param>
    /// <param name="options">Query options for paging and sorting.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A collection of matching radio stations.</returns>
    [HttpGet("stations/by-country-code/{countryCode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Station>>> GetStationsByCountryCode(
        string countryCode,
        [FromQuery] ListQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving stations by country code '{CountryCode}'", countryCode);

        var stations = await _radioBrowserClient.GetStationsByCountryCodeAsync(countryCode, options, cancellationToken);
        return Ok(stations);
    }

    /// <summary>
    /// Searches radio stations by state name or exact match.
    /// </summary>
    /// <param name="state">The state name.</param>
    /// <param name="exact">Whether to match the exact state string.</param>
    /// <param name="options">Query options for paging and sorting.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A collection of matching radio stations.</returns>
    [HttpGet("stations/by-state")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Station>>> GetStationsByState(
        [FromQuery, Required] string state,
        [FromQuery] bool exact = false,
        [FromQuery] ListQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving stations by state '{State}' (Exact: {Exact})", state, exact);

        var stations = exact
            ? await _radioBrowserClient.GetStationsByStateExactAsync(state, options, cancellationToken)
            : await _radioBrowserClient.GetStationsByStateAsync(state, options, cancellationToken);

        return Ok(stations);
    }

    /// <summary>
    /// Searches radio stations by language name or exact match.
    /// </summary>
    /// <param name="language">The language name.</param>
    /// <param name="exact">Whether to match the exact language string.</param>
    /// <param name="options">Query options for paging and sorting.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A collection of matching radio stations.</returns>
    [HttpGet("stations/by-language")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Station>>> GetStationsByLanguage(
        [FromQuery, Required] string language,
        [FromQuery] bool exact = false,
        [FromQuery] ListQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving stations by language '{Language}' (Exact: {Exact})", language, exact);

        var stations = exact
            ? await _radioBrowserClient.GetStationsByLanguageExactAsync(language, options, cancellationToken)
            : await _radioBrowserClient.GetStationsByLanguageAsync(language, options, cancellationToken);

        return Ok(stations);
    }

    /// <summary>
    /// Searches radio stations by audio codec string or exact match.
    /// </summary>
    /// <param name="codec">The audio codec name (e.g. MP3, AAC).</param>
    /// <param name="exact">Whether to match the exact codec string.</param>
    /// <param name="options">Query options for paging and sorting.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A collection of matching radio stations.</returns>
    [HttpGet("stations/by-codec")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Station>>> GetStationsByCodec(
        [FromQuery, Required] string codec,
        [FromQuery] bool exact = false,
        [FromQuery] ListQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving stations by codec '{Codec}' (Exact: {Exact})", codec, exact);

        var stations = exact
            ? await _radioBrowserClient.GetStationsByCodecExactAsync(codec, options, cancellationToken)
            : await _radioBrowserClient.GetStationsByCodecAsync(codec, options, cancellationToken);

        return Ok(stations);
    }

    /// <summary>
    /// Retrieves stations publishing a specific stream URL.
    /// </summary>
    /// <param name="url">The stream URL to match.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A collection of matching radio stations.</returns>
    [HttpGet("stations/by-url")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Station>>> GetStationsByUrl(
        [FromQuery, Required] string url,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving stations by stream URL '{Url}'", url);

        var stations = await _radioBrowserClient.GetStationsByUrlAsync(url, cancellationToken);
        return Ok(stations);
    }

    /// <summary>
    /// Gets the top clicked stations.
    /// </summary>
    [HttpGet("stations/top-clicked")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Station>>> GetTopClickedStations(
        [FromQuery] ListQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving top-clicked stations");

        var stations = await _radioBrowserClient.GetTopClickedStationsAsync(options, cancellationToken);
        return Ok(stations);
    }

    /// <summary>
    /// Gets the top voted stations.
    /// </summary>
    [HttpGet("stations/top-voted")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Station>>> GetTopVotedStations(
        [FromQuery] ListQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving top-voted stations");

        var stations = await _radioBrowserClient.GetTopVotedStationsAsync(options, cancellationToken);
        return Ok(stations);
    }

    /// <summary>
    /// Gets stations that were clicked most recently.
    /// </summary>
    [HttpGet("stations/recently-clicked")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Station>>> GetRecentlyClickedStations(
        [FromQuery] ListQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving recently-clicked stations");

        var stations = await _radioBrowserClient.GetRecentlyClickedStationsAsync(options, cancellationToken);
        return Ok(stations);
    }

    /// <summary>
    /// Gets stations whose metadata was modified most recently.
    /// </summary>
    [HttpGet("stations/recently-changed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Station>>> GetRecentlyChangedStations(
        [FromQuery] ListQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving recently-changed stations");

        var stations = await _radioBrowserClient.GetRecentlyChangedStationsAsync(options, cancellationToken);
        return Ok(stations);
    }

    /// <summary>
    /// Gets stations currently marked as broken.
    /// </summary>
    [HttpGet("stations/broken")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Station>>> GetBrokenStations(
        [FromQuery] ListQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving broken stations");

        var stations = await _radioBrowserClient.GetBrokenStationsAsync(options, cancellationToken);
        return Ok(stations);
    }

    /// <summary>
    /// Lists countries, optionally filtered by a substring.
    /// </summary>
    [HttpGet("countries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CountryInfo>>> GetCountries(
        [FromQuery] string? filter = null,
        [FromQuery] ListQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving countries list (Filter: {Filter})", filter);

        var countries = await _radioBrowserClient.GetCountriesAsync(filter, options, cancellationToken);
        return Ok(countries);
    }

    /// <summary>
    /// Lists ISO country codes and station counts.
    /// </summary>
    [HttpGet("country-codes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CountryCodeInfo>>> GetCountryCodes(
        [FromQuery] string? filter = null,
        [FromQuery] ListQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving country codes (Filter: {Filter})", filter);

        var codes = await _radioBrowserClient.GetCountryCodesAsync(filter, options, cancellationToken);
        return Ok(codes);
    }

    /// <summary>
    /// Lists states, optionally scoped to a country and name filter.
    /// </summary>
    [HttpGet("states")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StateInfo>>> GetStates(
        [FromQuery] string? country = null,
        [FromQuery] string? filter = null,
        [FromQuery] ListQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving states list (Country: {Country}, Filter: {Filter})", country, filter);

        var states = await _radioBrowserClient.GetStatesAsync(country, filter, options, cancellationToken);
        return Ok(states);
    }

    /// <summary>
    /// Lists languages, optionally filtered by name.
    /// </summary>
    [HttpGet("languages")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LanguageInfo>>> GetLanguages(
        [FromQuery] string? filter = null,
        [FromQuery] ListQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving languages list (Filter: {Filter})", filter);

        var languages = await _radioBrowserClient.GetLanguagesAsync(filter, options, cancellationToken);
        return Ok(languages);
    }

    /// <summary>
    /// Lists tags, optionally filtered by name.
    /// </summary>
    [HttpGet("tags")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TagInfo>>> GetTags(
        [FromQuery] string? filter = null,
        [FromQuery] ListQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving tags list (Filter: {Filter})", filter);

        var tags = await _radioBrowserClient.GetTagsAsync(filter, options, cancellationToken);
        return Ok(tags);
    }

    /// <summary>
    /// Lists codecs, optionally filtered by name.
    /// </summary>
    [HttpGet("codecs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CodecInfo>>> GetCodecs(
        [FromQuery] string? filter = null,
        [FromQuery] ListQueryOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving codecs list (Filter: {Filter})", filter);

        var codecs = await _radioBrowserClient.GetCodecsAsync(filter, options, cancellationToken);
        return Ok(codecs);
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

        var result = await _radioBrowserClient.ClickStationAsync(stationUuid, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Submits a vote for a specific station.
    /// </summary>
    /// <param name="stationUuid">The unique identifier of the target station.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Vote outcome details.</returns>
    [HttpPost("stations/{stationUuid}/vote")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<VoteResult>> VoteStation(
        string stationUuid,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Casting vote for station UUID {StationUuid}", stationUuid);

        var result = await _radioBrowserClient.VoteStationAsync(stationUuid, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Submits a new radio station to the global index.
    /// </summary>
    /// <param name="request">Parameters for adding the station.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>Creation result status.</returns>
    [HttpPost("stations")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<AddStationResult>> AddStation(
        [FromBody] AddStationRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Submitting new station request for '{StationName}'", request.Name);

        var result = await _radioBrowserClient.AddStationAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetStationByUuid), new { stationUuid = result.Uuid }, result);
    }

    /// <summary>
    /// Retrieves recent click log events.
    /// </summary>
    /// <param name="stationUuid">Optional station UUID filter.</param>
    /// <param name="seconds">Lookback duration in seconds.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A list of click history records.</returns>
    [HttpGet("clicks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StationClick>>> GetClicks(
        [FromQuery] string? stationUuid = null,
        [FromQuery] int? seconds = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving click logs");

        var clicks = await _radioBrowserClient.GetClicksAsync(stationUuid, seconds, cancellationToken);
        return Ok(clicks);
    }

    /// <summary>
    /// Retrieves stream health-check logs.
    /// </summary>
    /// <param name="stationUuid">Optional station UUID filter.</param>
    /// <param name="lastCheckTime">Filter by check time constraint.</param>
    /// <param name="seconds">Lookback duration in seconds.</param>
    /// <param name="cancellationToken">Cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A list of check history records.</returns>
    [HttpGet("checks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<StationCheck>>> GetChecks(
        [FromQuery] string? stationUuid = null,
        [FromQuery] int? lastCheckTime = null,
        [FromQuery] int? seconds = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving stream check logs");

        var checks = await _radioBrowserClient.GetChecksAsync(stationUuid, lastCheckTime, seconds, cancellationToken);
        return Ok(checks);
    }

    /// <summary>
    /// Builds a formatted playlist URL (e.g., M3U, PLS, XSPF) for a target route on the Radio Browser mirror.
    /// </summary>
    /// <param name="format">The target extension or format (e.g., m3u, pls, xspf).</param>
    /// <param name="relativePathAndQuery">The relative path and query string parameters.</param>
    /// <returns>The fully qualified playlist Uri.</returns>
    [HttpGet("playlist-url")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<Uri> GetPlaylistUrl(
        [FromQuery, Required] string format,
        [FromQuery, Required] string relativePathAndQuery)
    {
        _logger.LogInformation("Generating playlist URL for format {Format}", format);

        var playlistUri = _radioBrowserClient.GetPlaylistUri(format, relativePathAndQuery);
        return Ok(playlistUri);
    }
}
