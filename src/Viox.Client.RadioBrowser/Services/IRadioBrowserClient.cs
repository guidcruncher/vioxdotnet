using Viox.Client.RadioBrowser.Models;

namespace Viox.Client.RadioBrowser.Services;

/// <summary>
/// Typed client for the Radio Browser REST API at radio-browser.info.
/// </summary>
public interface IRadioBrowserClient
{
    /// <summary>Gets the HTTP origin currently used by this client.</summary>
    Uri BaseAddress { get; }

    /// <summary>Gets live statistics for the connected mirror.</summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    Task<ServerStats> GetStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets mirrors advertised by the current server.</summary>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    Task<IReadOnlyList<ApiServer>> GetServersAsync(CancellationToken cancellationToken = default);

    /// <summary>Searches stations with optional multi-field filters.</summary>
    /// <param name="options">Search filters and paging. When <see langword="null"/>, the first page of all stations is requested.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    Task<IReadOnlyList<Station>> SearchStationsAsync(
        StationSearchOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Lists stations without additional filters.</summary>
    /// <param name="options">Paging and sort options.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    Task<IReadOnlyList<Station>> GetStationsAsync(
        ListQueryOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>Gets stations by one or more UUIDs.</summary>
    /// <param name="stationUuids">One or more station UUIDs.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    Task<IReadOnlyList<Station>> GetStationsByUuidAsync(
        IEnumerable<string> stationUuids,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a single station by UUID, or <see langword="null"/> when it does not exist.</summary>
    /// <param name="stationUuid">Station UUID.</param>
    /// <param name="cancellationToken">Token used to cancel the request.</param>
    Task<Station?> GetStationByUuidAsync(string stationUuid, CancellationToken cancellationToken = default);

    /// <summary>Finds stations whose name contains <paramref name="name"/>.</summary>
    Task<IReadOnlyList<Station>> GetStationsByNameAsync(string name, ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Finds stations whose name equals <paramref name="name"/>.</summary>
    Task<IReadOnlyList<Station>> GetStationsByNameExactAsync(string name, ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Finds stations whose tags contain <paramref name="tag"/>.</summary>
    Task<IReadOnlyList<Station>> GetStationsByTagAsync(string tag, ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Finds stations that have the exact tag <paramref name="tag"/>.</summary>
    Task<IReadOnlyList<Station>> GetStationsByTagExactAsync(string tag, ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Finds stations whose country name contains <paramref name="country"/>.</summary>
    Task<IReadOnlyList<Station>> GetStationsByCountryAsync(string country, ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Finds stations whose country name equals <paramref name="country"/>.</summary>
    Task<IReadOnlyList<Station>> GetStationsByCountryExactAsync(string country, ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Finds stations by ISO 3166-1 alpha-2 country code.</summary>
    Task<IReadOnlyList<Station>> GetStationsByCountryCodeAsync(string countryCode, ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Finds stations whose state contains <paramref name="state"/>.</summary>
    Task<IReadOnlyList<Station>> GetStationsByStateAsync(string state, ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Finds stations whose state equals <paramref name="state"/>.</summary>
    Task<IReadOnlyList<Station>> GetStationsByStateExactAsync(string state, ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Finds stations whose language contains <paramref name="language"/>.</summary>
    Task<IReadOnlyList<Station>> GetStationsByLanguageAsync(string language, ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Finds stations whose language equals <paramref name="language"/>.</summary>
    Task<IReadOnlyList<Station>> GetStationsByLanguageExactAsync(string language, ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Finds stations whose codec contains <paramref name="codec"/>.</summary>
    Task<IReadOnlyList<Station>> GetStationsByCodecAsync(string codec, ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Finds stations whose codec equals <paramref name="codec"/>.</summary>
    Task<IReadOnlyList<Station>> GetStationsByCodecExactAsync(string codec, ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Finds stations that publish the given stream URL.</summary>
    Task<IReadOnlyList<Station>> GetStationsByUrlAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>Gets the most-clicked stations.</summary>
    Task<IReadOnlyList<Station>> GetTopClickedStationsAsync(ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the highest-voted stations.</summary>
    Task<IReadOnlyList<Station>> GetTopVotedStationsAsync(ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Gets stations that were clicked most recently.</summary>
    Task<IReadOnlyList<Station>> GetRecentlyClickedStationsAsync(ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Gets stations whose metadata changed most recently.</summary>
    Task<IReadOnlyList<Station>> GetRecentlyChangedStationsAsync(ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Gets stations currently marked as broken.</summary>
    Task<IReadOnlyList<Station>> GetBrokenStationsAsync(ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Lists countries, optionally filtered by a name substring.</summary>
    Task<IReadOnlyList<CountryInfo>> GetCountriesAsync(string? filter = null, ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Lists ISO country codes and station counts.</summary>
    Task<IReadOnlyList<CountryCodeInfo>> GetCountryCodesAsync(string? filter = null, ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Lists states, optionally scoped to a country and name filter.</summary>
    Task<IReadOnlyList<StateInfo>> GetStatesAsync(string? country = null, string? filter = null, ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Lists languages, optionally filtered by a name substring.</summary>
    Task<IReadOnlyList<LanguageInfo>> GetLanguagesAsync(string? filter = null, ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Lists tags, optionally filtered by a name substring.</summary>
    Task<IReadOnlyList<TagInfo>> GetTagsAsync(string? filter = null, ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Lists codecs, optionally filtered by a name substring.</summary>
    Task<IReadOnlyList<CodecInfo>> GetCodecsAsync(string? filter = null, ListQueryOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a play/click for <paramref name="stationUuid"/> and returns the resolved stream URL.
    /// Clients should call this when the user starts playback. The API counts at most one click
    /// per IP per station per 24 hours.
    /// </summary>
    Task<ClickResult> ClickStationAsync(string stationUuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Casts a vote for <paramref name="stationUuid"/>. The API typically accepts one vote per IP
    /// per station every 10 minutes.
    /// </summary>
    Task<VoteResult> VoteStationAsync(string stationUuid, CancellationToken cancellationToken = default);

    /// <summary>Submits a new station to the directory.</summary>
    Task<AddStationResult> AddStationAsync(AddStationRequest request, CancellationToken cancellationToken = default);

    /// <summary>Gets recent click events, optionally limited to one station.</summary>
    Task<IReadOnlyList<StationClick>> GetClicksAsync(string? stationUuid = null, int? seconds = null, CancellationToken cancellationToken = default);

    /// <summary>Gets recent stream health-check results, optionally limited to one station.</summary>
    Task<IReadOnlyList<StationCheck>> GetChecksAsync(string? stationUuid = null, int? lastCheckTime = null, int? seconds = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds a playlist URL on the current mirror. The caller can download M3U, PLS, XSPF, or TTL
    /// from the same routes used for JSON by swapping the format prefix.
    /// </summary>
    Uri GetPlaylistUri(string format, string relativePathAndQuery);
}
