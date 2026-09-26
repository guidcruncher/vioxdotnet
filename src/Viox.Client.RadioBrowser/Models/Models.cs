using System.Text.Json.Serialization;

using Viox.Client.RadioBrowser.Services;

namespace Viox.Client.RadioBrowser.Models;

/// <summary>
/// Sort keys accepted by Radio Browser list and search endpoints.
/// </summary>
public enum StationOrder
{
    /// <summary>Sort by station or list name.</summary>
    Name,

    /// <summary>Sort by stream URL.</summary>
    Url,

    /// <summary>Sort by homepage URL.</summary>
    Homepage,

    /// <summary>Sort by favicon URL.</summary>
    Favicon,

    /// <summary>Sort by tag string.</summary>
    Tags,

    /// <summary>Sort by country name.</summary>
    Country,

    /// <summary>Sort by state or region.</summary>
    State,

    /// <summary>Sort by language string.</summary>
    Language,

    /// <summary>Sort by vote count.</summary>
    Votes,

    /// <summary>Sort by codec name.</summary>
    Codec,

    /// <summary>Sort by bitrate.</summary>
    Bitrate,

    /// <summary>Sort by last successful health-check flag.</summary>
    LastCheckOk,

    /// <summary>Sort by last health-check time.</summary>
    LastCheckTime,

    /// <summary>Sort by last recorded click time.</summary>
    ClickTimestamp,

    /// <summary>Sort by click count.</summary>
    ClickCount,

    /// <summary>Sort by recent click trend.</summary>
    ClickTrend,

    /// <summary>Sort by last metadata change time.</summary>
    ChangeTimestamp,

    /// <summary>Return rows in a random order.</summary>
    Random,

    /// <summary>Sort metadata lists by the number of stations.</summary>
    StationCount
}

/// <summary>
/// Shared pagination and sorting options for list endpoints.
/// </summary>
public class ListQueryOptions
{
    /// <summary>Gets or sets the field used to sort results.</summary>
    public StationOrder? Order { get; set; }

    /// <summary>Gets or sets whether the sort order is reversed.</summary>
    public bool? Reverse { get; set; }

    /// <summary>Gets or sets the zero-based offset for server-side paging.</summary>
    public int? Offset { get; set; }

    /// <summary>Gets or sets the maximum number of rows to return.</summary>
    public int? Limit { get; set; }

    /// <summary>Gets or sets whether stations that failed the last health check are omitted.</summary>
    public bool? HideBroken { get; set; }
}

/// <summary>
/// Filter and paging options for <c>/json/stations/search</c>.
/// Every property is optional; unset properties are omitted from the query string.
/// </summary>
public sealed class StationSearchOptions : ListQueryOptions
{
    /// <summary>Gets or sets a station name fragment. Matching is case-insensitive unless <see cref="NameExact"/> is set.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets whether <see cref="Name"/> must match exactly.</summary>
    public bool? NameExact { get; set; }

    /// <summary>Gets or sets a country name filter.</summary>
    public string? Country { get; set; }

    /// <summary>Gets or sets whether <see cref="Country"/> must match exactly.</summary>
    public bool? CountryExact { get; set; }

    /// <summary>Gets or sets an ISO 3166-1 alpha-2 country code such as <c>US</c> or <c>DE</c>.</summary>
    public string? CountryCode { get; set; }

    /// <summary>Gets or sets a state or region name filter.</summary>
    public string? State { get; set; }

    /// <summary>Gets or sets whether <see cref="State"/> must match exactly.</summary>
    public bool? StateExact { get; set; }

    /// <summary>Gets or sets a spoken-language filter.</summary>
    public string? Language { get; set; }

    /// <summary>Gets or sets whether <see cref="Language"/> must match exactly.</summary>
    public bool? LanguageExact { get; set; }

    /// <summary>Gets or sets a single tag / genre filter.</summary>
    public string? Tag { get; set; }

    /// <summary>Gets or sets whether <see cref="Tag"/> must match exactly.</summary>
    public bool? TagExact { get; set; }

    /// <summary>Gets or sets a comma-separated list of tags that must all be present.</summary>
    public string? TagList { get; set; }

    /// <summary>Gets or sets a codec filter such as <c>MP3</c> or <c>AAC</c>.</summary>
    public string? Codec { get; set; }

    /// <summary>Gets or sets the minimum bitrate in kbps.</summary>
    public int? BitrateMin { get; set; }

    /// <summary>Gets or sets the maximum bitrate in kbps.</summary>
    public int? BitrateMax { get; set; }

    /// <summary>Gets or sets whether results must include geographic coordinates. <see langword="null"/> means either.</summary>
    public bool? HasGeoInfo { get; set; }

    /// <summary>Gets or sets whether results must advertise extended HTTP stream info. <see langword="null"/> means either.</summary>
    public bool? HasExtendedInfo { get; set; }

    /// <summary>Gets or sets whether results must use HTTPS stream URLs. <see langword="null"/> means either.</summary>
    public bool? IsHttps { get; set; }

    /// <summary>Gets or sets a reference latitude used to populate <see cref="Station.GeoDistance"/>.</summary>
    public double? GeoLatitude { get; set; }

    /// <summary>Gets or sets a reference longitude used to populate <see cref="Station.GeoDistance"/>.</summary>
    public double? GeoLongitude { get; set; }
}

/// <summary>
/// Fields accepted by <c>POST /json/add</c> when submitting a new station.
/// </summary>
public sealed class AddStationRequest
{
    /// <summary>Gets or sets the display name. Required by the API.</summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets the stream URL. Required by the API.</summary>
    public required string Url { get; set; }

    /// <summary>Gets or sets the station homepage URL.</summary>
    public string? Homepage { get; set; }

    /// <summary>Gets or sets the favicon URL.</summary>
    public string? Favicon { get; set; }

    /// <summary>Gets or sets the ISO 3166-1 alpha-2 country code.</summary>
    public string? CountryCode { get; set; }

    /// <summary>Gets or sets the state or region.</summary>
    public string? State { get; set; }

    /// <summary>Gets or sets a comma-separated language list.</summary>
    public string? Language { get; set; }

    /// <summary>Gets or sets a comma-separated tag list.</summary>
    public string? Tags { get; set; }

    /// <summary>Gets or sets the geographic latitude.</summary>
    public double? GeoLatitude { get; set; }

    /// <summary>Gets or sets the geographic longitude.</summary>
    public double? GeoLongitude { get; set; }
}

/// <summary>
/// An internet radio station as returned by Radio Browser.
/// </summary>
public sealed class Station
{

    /// <summary>Uri of station</summary>
    [JsonPropertyName("uri")]
    public string Uri { get => $"radiobrowser:station:{StationUuid}"; }

    /// <summary>Gets or sets the unique identifier of the last metadata change.</summary>
    [JsonPropertyName("changeuuid")]
    public string? ChangeUuid { get; set; }

    /// <summary>Gets or sets the globally unique station identifier.</summary>
    [JsonPropertyName("stationuuid")]
    public string? StationUuid { get; set; }

    /// <summary>Gets or sets the identifier of the server that last modified the row.</summary>
    [JsonPropertyName("serveruuid")]
    public string? ServerUuid { get; set; }

    /// <summary>Gets or sets the display name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Gets or sets the original stream URL supplied by the station owner.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the resolved playable URL after playlist and redirect following.
    /// Prefer this value for playback after recording a click.
    /// </summary>
    [JsonPropertyName("url_resolved")]
    public string? UrlResolved { get; set; }

    /// <summary>Gets or sets the station homepage.</summary>
    [JsonPropertyName("homepage")]
    public string? Homepage { get; set; }

    /// <summary>Gets or sets the favicon or logo URL.</summary>
    [JsonPropertyName("favicon")]
    public string? Favicon { get; set; }

    /// <summary>Gets or sets a comma-separated list of tags.</summary>
    [JsonPropertyName("tags")]
    public string? Tags { get; set; }

    /// <summary>Gets or sets the localized country name. Prefer <see cref="CountryCode"/>.</summary>
    [JsonPropertyName("country")]
    public string? Country { get; set; }

    /// <summary>Gets or sets the ISO 3166-1 alpha-2 country code.</summary>
    [JsonPropertyName("countrycode")]
    public string? CountryCode { get; set; }

    /// <summary>Gets or sets the ISO 3166-2 subdivision code when known.</summary>
    [JsonPropertyName("iso_3166_2")]
    public string? Iso3166_2 { get; set; }

    /// <summary>Gets or sets the state or region name.</summary>
    [JsonPropertyName("state")]
    public string? State { get; set; }

    /// <summary>Gets or sets a comma-separated list of spoken languages.</summary>
    [JsonPropertyName("language")]
    public string? Language { get; set; }

    /// <summary>Gets or sets comma-separated ISO 639 language codes.</summary>
    [JsonPropertyName("languagecodes")]
    public string? LanguageCodes { get; set; }

    /// <summary>Gets or sets the lifetime vote count.</summary>
    [JsonPropertyName("votes")]
    [JsonConverter(typeof(FlexibleInt32Converter))]
    public int Votes { get; set; }

    /// <summary>Gets or sets the last change time in server local format.</summary>
    [JsonPropertyName("lastchangetime")]
    public string? LastChangeTime { get; set; }

    /// <summary>Gets or sets the last change time in ISO 8601.</summary>
    [JsonPropertyName("lastchangetime_iso8601")]
    public DateTimeOffset? LastChangeTimeIso8601 { get; set; }

    /// <summary>Gets or sets the detected audio codec.</summary>
    [JsonPropertyName("codec")]
    public string? Codec { get; set; }

    /// <summary>Gets or sets the detected bitrate in kbps.</summary>
    [JsonPropertyName("bitrate")]
    [JsonConverter(typeof(FlexibleInt32Converter))]
    public int Bitrate { get; set; }

    /// <summary>Gets or sets whether the stream is HLS.</summary>
    [JsonPropertyName("hls")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool Hls { get; set; }

    /// <summary>Gets or sets whether the last majority health check succeeded.</summary>
    [JsonPropertyName("lastcheckok")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool LastCheckOk { get; set; }

    /// <summary>Gets or sets the last health-check time in server local format.</summary>
    [JsonPropertyName("lastchecktime")]
    public string? LastCheckTime { get; set; }

    /// <summary>Gets or sets the last health-check time in ISO 8601.</summary>
    [JsonPropertyName("lastchecktime_iso8601")]
    public DateTimeOffset? LastCheckTimeIso8601 { get; set; }

    /// <summary>Gets or sets the last successful health-check time in server local format.</summary>
    [JsonPropertyName("lastcheckoktime")]
    public string? LastCheckOkTime { get; set; }

    /// <summary>Gets or sets the last successful health-check time in ISO 8601.</summary>
    [JsonPropertyName("lastcheckoktime_iso8601")]
    public DateTimeOffset? LastCheckOkTimeIso8601 { get; set; }

    /// <summary>Gets or sets the last local-server check time in server local format.</summary>
    [JsonPropertyName("lastlocalchecktime")]
    public string? LastLocalCheckTime { get; set; }

    /// <summary>Gets or sets the last local-server check time in ISO 8601.</summary>
    [JsonPropertyName("lastlocalchecktime_iso8601")]
    public DateTimeOffset? LastLocalCheckTimeIso8601 { get; set; }

    /// <summary>Gets or sets the last click time in server local format.</summary>
    [JsonPropertyName("clicktimestamp")]
    public string? ClickTimestamp { get; set; }

    /// <summary>Gets or sets the last click time in ISO 8601.</summary>
    [JsonPropertyName("clicktimestamp_iso8601")]
    public DateTimeOffset? ClickTimestampIso8601 { get; set; }

    /// <summary>Gets or sets the number of clicks recorded in the last 24 hours.</summary>
    [JsonPropertyName("clickcount")]
    [JsonConverter(typeof(FlexibleInt32Converter))]
    public int ClickCount { get; set; }

    /// <summary>Gets or sets the two-day click-count delta. Positive values are trending up.</summary>
    [JsonPropertyName("clicktrend")]
    [JsonConverter(typeof(FlexibleInt32Converter))]
    public int ClickTrend { get; set; }

    /// <summary>Gets or sets whether connecting to the stream URL produced an SSL error.</summary>
    [JsonPropertyName("ssl_error")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool SslError { get; set; }

    /// <summary>Gets or sets the station latitude when known.</summary>
    [JsonPropertyName("geo_lat")]
    [JsonConverter(typeof(FlexibleNullableDoubleConverter))]
    public double? GeoLatitude { get; set; }

    /// <summary>Gets or sets the station longitude when known.</summary>
    [JsonPropertyName("geo_long")]
    [JsonConverter(typeof(FlexibleNullableDoubleConverter))]
    public double? GeoLongitude { get; set; }

    /// <summary>Gets or sets the distance in metres from the coordinates supplied in the search request.</summary>
    [JsonPropertyName("geo_distance")]
    [JsonConverter(typeof(FlexibleNullableDoubleConverter))]
    public double? GeoDistance { get; set; }

    /// <summary>Gets or sets whether the live stream advertises extended metadata that overrides the database.</summary>
    [JsonPropertyName("has_extended_info")]
    [JsonConverter(typeof(FlexibleNullableBooleanConverter))]
    public bool? HasExtendedInfo { get; set; }

    /// <summary>
    /// Splits <see cref="Tags"/> into individual values.
    /// </summary>
    /// <returns>Tag tokens with empty entries removed.</returns>
    public IReadOnlyList<string> GetTagList() => SplitCsv(Tags);

    /// <summary>
    /// Splits <see cref="Language"/> into individual values.
    /// </summary>
    /// <returns>Language tokens with empty entries removed.</returns>
    public IReadOnlyList<string> GetLanguageList() => SplitCsv(Language);

    private static IReadOnlyList<string> SplitCsv(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}

/// <summary>A country name and the number of stations attributed to it.</summary>
public sealed class CountryInfo
{
    /// <summary>Gets or sets the country name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Gets or sets the ISO 3166-1 alpha-2 code when the endpoint returns it.</summary>
    [JsonPropertyName("iso_3166_1")]
    public string? Iso3166_1 { get; set; }

    /// <summary>Gets or sets the station count.</summary>
    [JsonPropertyName("stationcount")]
    [JsonConverter(typeof(FlexibleInt32Converter))]
    public int StationCount { get; set; }
}

/// <summary>An ISO country code and the number of stations attributed to it.</summary>
public sealed class CountryCodeInfo
{
    /// <summary>Gets or sets the two-letter country code.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Gets or sets the station count.</summary>
    [JsonPropertyName("stationcount")]
    [JsonConverter(typeof(FlexibleInt32Converter))]
    public int StationCount { get; set; }
}

/// <summary>A state or region and the number of stations attributed to it.</summary>
public sealed class StateInfo
{
    /// <summary>Gets or sets the state or region name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Gets or sets the country name.</summary>
    [JsonPropertyName("country")]
    public string? Country { get; set; }

    /// <summary>Gets or sets the station count.</summary>
    [JsonPropertyName("stationcount")]
    [JsonConverter(typeof(FlexibleInt32Converter))]
    public int StationCount { get; set; }
}

/// <summary>A language and the number of stations attributed to it.</summary>
public sealed class LanguageInfo
{
    /// <summary>Gets or sets the language name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Gets or sets the ISO 639 code when known.</summary>
    [JsonPropertyName("iso_639")]
    public string? Iso639 { get; set; }

    /// <summary>Gets or sets the station count.</summary>
    [JsonPropertyName("stationcount")]
    [JsonConverter(typeof(FlexibleInt32Converter))]
    public int StationCount { get; set; }
}

/// <summary>A tag / genre and the number of stations attributed to it.</summary>
public sealed class TagInfo
{
    /// <summary>Gets or sets the tag name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Gets or sets the station count.</summary>
    [JsonPropertyName("stationcount")]
    [JsonConverter(typeof(FlexibleInt32Converter))]
    public int StationCount { get; set; }
}

/// <summary>An audio codec and the number of stations that use it.</summary>
public sealed class CodecInfo
{
    /// <summary>Gets or sets the codec name, for example <c>MP3</c>.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Gets or sets the station count.</summary>
    [JsonPropertyName("stationcount")]
    [JsonConverter(typeof(FlexibleInt32Converter))]
    public int StationCount { get; set; }
}

/// <summary>Live statistics published by an API mirror.</summary>
public sealed class ServerStats
{
    /// <summary>Gets or sets the highest supported web-service version.</summary>
    [JsonPropertyName("supported_version")]
    [JsonConverter(typeof(FlexibleInt32Converter))]
    public int SupportedVersion { get; set; }

    /// <summary>Gets or sets the server software version string.</summary>
    [JsonPropertyName("software_version")]
    public string? SoftwareVersion { get; set; }

    /// <summary>Gets or sets the server status, typically <c>OK</c>.</summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>Gets or sets the number of stations in the database.</summary>
    [JsonPropertyName("stations")]
    [JsonConverter(typeof(FlexibleInt32Converter))]
    public int Stations { get; set; }

    /// <summary>Gets or sets the number of stations currently marked broken.</summary>
    [JsonPropertyName("stations_broken")]
    [JsonConverter(typeof(FlexibleInt32Converter))]
    public int StationsBroken { get; set; }

    /// <summary>Gets or sets the number of distinct tags.</summary>
    [JsonPropertyName("tags")]
    [JsonConverter(typeof(FlexibleInt32Converter))]
    public int Tags { get; set; }

    /// <summary>Gets or sets clicks recorded in the last hour.</summary>
    [JsonPropertyName("clicks_last_hour")]
    [JsonConverter(typeof(FlexibleInt32Converter))]
    public int ClicksLastHour { get; set; }

    /// <summary>Gets or sets clicks recorded in the last day.</summary>
    [JsonPropertyName("clicks_last_day")]
    [JsonConverter(typeof(FlexibleInt32Converter))]
    public int ClicksLastDay { get; set; }

    /// <summary>Gets or sets the number of distinct languages.</summary>
    [JsonPropertyName("languages")]
    [JsonConverter(typeof(FlexibleInt32Converter))]
    public int Languages { get; set; }

    /// <summary>Gets or sets the number of distinct countries.</summary>
    [JsonPropertyName("countries")]
    [JsonConverter(typeof(FlexibleInt32Converter))]
    public int Countries { get; set; }
}

/// <summary>A Radio Browser API mirror advertised by <c>/json/servers</c>.</summary>
public sealed class ApiServer
{
    /// <summary>Gets or sets the server IPv4 or IPv6 address.</summary>
    [JsonPropertyName("ip")]
    public string? Ip { get; set; }

    /// <summary>Gets or sets the DNS host name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

/// <summary>Result of <c>GET /json/url/{uuid}</c>.</summary>
public sealed class ClickResult
{
    /// <summary>Gets or sets whether the click was accepted.</summary>
    [JsonPropertyName("ok")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool Ok { get; set; }

    /// <summary>Gets or sets a server message.</summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>Gets or sets the station UUID.</summary>
    [JsonPropertyName("stationuuid")]
    public string? StationUuid { get; set; }

    /// <summary>Gets or sets the station name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Gets or sets the resolved stream URL that should be used for playback.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

/// <summary>Result of <c>GET /json/vote/{uuid}</c>.</summary>
public sealed class VoteResult
{
    /// <summary>Gets or sets whether the vote was accepted.</summary>
    [JsonPropertyName("ok")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool Ok { get; set; }

    /// <summary>Gets or sets a server message. Duplicate votes typically return an explanatory message.</summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

/// <summary>Result of <c>POST /json/add</c>.</summary>
public sealed class AddStationResult
{
    /// <summary>Gets or sets whether the station was accepted.</summary>
    [JsonPropertyName("ok")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool Ok { get; set; }

    /// <summary>Gets or sets a server message.</summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>Gets or sets the UUID assigned to the new station when creation succeeded.</summary>
    [JsonPropertyName("uuid")]
    public string? Uuid { get; set; }
}

/// <summary>A recorded click event from <c>/json/clicks</c>.</summary>
public sealed class StationClick
{
    /// <summary>Gets or sets the station UUID.</summary>
    [JsonPropertyName("stationuuid")]
    public string? StationUuid { get; set; }

    /// <summary>Gets or sets the click event UUID.</summary>
    [JsonPropertyName("clickuuid")]
    public string? ClickUuid { get; set; }

    /// <summary>Gets or sets the click time in server local format.</summary>
    [JsonPropertyName("clicktimestamp")]
    public string? ClickTimestamp { get; set; }

    /// <summary>Gets or sets the click time in ISO 8601.</summary>
    [JsonPropertyName("clicktimestamp_iso8601")]
    public DateTimeOffset? ClickTimestampIso8601 { get; set; }
}

/// <summary>A stream health-check result from <c>/json/checks</c>.</summary>
public sealed class StationCheck
{
    /// <summary>Gets or sets the check UUID.</summary>
    [JsonPropertyName("checkuuid")]
    public string? CheckUuid { get; set; }

    /// <summary>Gets or sets the station UUID.</summary>
    [JsonPropertyName("stationuuid")]
    public string? StationUuid { get; set; }

    /// <summary>Gets or sets the source URL that was checked.</summary>
    [JsonPropertyName("source_url")]
    public string? SourceUrl { get; set; }

    /// <summary>Gets or sets the detected codec.</summary>
    [JsonPropertyName("codec")]
    public string? Codec { get; set; }

    /// <summary>Gets or sets the detected bitrate.</summary>
    [JsonPropertyName("bitrate")]
    [JsonConverter(typeof(FlexibleInt32Converter))]
    public int Bitrate { get; set; }

    /// <summary>Gets or sets whether the check succeeded.</summary>
    [JsonPropertyName("ok")]
    [JsonConverter(typeof(FlexibleBooleanConverter))]
    public bool Ok { get; set; }

    /// <summary>Gets or sets whether the URL used HTTPS.</summary>
    [JsonPropertyName("urlcache")]
    public string? UrlCache { get; set; }

    /// <summary>Gets or sets the check timestamp in server local format.</summary>
    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    /// <summary>Gets or sets the check timestamp in ISO 8601.</summary>
    [JsonPropertyName("timestamp_iso8601")]
    public DateTimeOffset? TimestampIso8601 { get; set; }
}
