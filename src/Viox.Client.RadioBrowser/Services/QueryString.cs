using System.Globalization;
using System.Text;

using Viox.Client.RadioBrowser.Models;

namespace Viox.Client.RadioBrowser.Services;

internal static class QueryString
{
    public static string Build(params (string Name, string? Value)[] values)
    {
        var builder = new StringBuilder();
        foreach (var (name, value) in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            builder.Append(builder.Length == 0 ? '?' : '&');
            builder.Append(Uri.EscapeDataString(name));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(value));
        }

        return builder.ToString();
    }

    public static string FromListOptions(ListQueryOptions? options)
    {
        options ??= new ListQueryOptions();
        return Build(
            ("order", options.Order is null ? null : ToApiOrder(options.Order.Value)),
            ("reverse", ToApiBoolean(options.Reverse)),
            ("offset", ToApiInt(options.Offset)),
            ("limit", ToApiInt(options.Limit)),
            ("hidebroken", ToApiBoolean(options.HideBroken)));
    }

    public static string FromSearchOptions(StationSearchOptions? options)
    {
        options ??= new StationSearchOptions();
        return Build(
            ("name", options.Name),
            ("nameExact", ToApiBoolean(options.NameExact)),
            ("country", options.Country),
            ("countryExact", ToApiBoolean(options.CountryExact)),
            ("countrycode", options.CountryCode),
            ("state", options.State),
            ("stateExact", ToApiBoolean(options.StateExact)),
            ("language", options.Language),
            ("languageExact", ToApiBoolean(options.LanguageExact)),
            ("tag", options.Tag),
            ("tagExact", ToApiBoolean(options.TagExact)),
            ("tagList", options.TagList),
            ("codec", options.Codec),
            ("bitrateMin", ToApiInt(options.BitrateMin)),
            ("bitrateMax", ToApiInt(options.BitrateMax)),
            ("has_geo_info", ToApiTriState(options.HasGeoInfo)),
            ("has_extended_info", ToApiTriState(options.HasExtendedInfo)),
            ("is_https", ToApiTriState(options.IsHttps)),
            ("geo_lat", ToApiDouble(options.GeoLatitude)),
            ("geo_long", ToApiDouble(options.GeoLongitude)),
            ("order", options.Order is null ? null : ToApiOrder(options.Order.Value)),
            ("reverse", ToApiBoolean(options.Reverse)),
            ("offset", ToApiInt(options.Offset)),
            ("limit", ToApiInt(options.Limit)),
            ("hidebroken", ToApiBoolean(options.HideBroken)));
    }

    public static string Combine(string path, string query)
        => string.IsNullOrEmpty(query) ? path : path + query;

    public static string EscapePath(string value)
        => Uri.EscapeDataString(value);

    internal static string ToApiOrder(StationOrder order) => order switch
    {
        StationOrder.Name => "name",
        StationOrder.Url => "url",
        StationOrder.Homepage => "homepage",
        StationOrder.Favicon => "favicon",
        StationOrder.Tags => "tags",
        StationOrder.Country => "country",
        StationOrder.State => "state",
        StationOrder.Language => "language",
        StationOrder.Votes => "votes",
        StationOrder.Codec => "codec",
        StationOrder.Bitrate => "bitrate",
        StationOrder.LastCheckOk => "lastcheckok",
        StationOrder.LastCheckTime => "lastchecktime",
        StationOrder.ClickTimestamp => "clicktimestamp",
        StationOrder.ClickCount => "clickcount",
        StationOrder.ClickTrend => "clicktrend",
        StationOrder.ChangeTimestamp => "changetimestamp",
        StationOrder.Random => "random",
        StationOrder.StationCount => "stationcount",
        _ => throw new ArgumentOutOfRangeException(nameof(order), order, "Unsupported station order.")
    };

    internal static string? ToApiBoolean(bool? value)
        => value is null ? null : (value.Value ? "true" : "false");

    internal static string? ToApiTriState(bool? value)
        => value is null ? null : (value.Value ? "true" : "false");

    internal static string? ToApiInt(int? value)
        => value is null ? null : value.Value.ToString(CultureInfo.InvariantCulture);

    internal static string? ToApiDouble(double? value)
        => value is null ? null : value.Value.ToString("G", CultureInfo.InvariantCulture);

    internal static FormUrlEncodedContent ToForm(IEnumerable<(string Name, string? Value)> values)
    {
        var pairs = values
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
            .Select(pair => new KeyValuePair<string, string>(pair.Name, pair.Value!));
        return new FormUrlEncodedContent(pairs);
    }
}
