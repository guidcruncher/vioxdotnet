using System.Text.Json;

namespace Viox.Client.Youtube.Internal;

internal static class JsonElementExtensions
{
    public static IEnumerable<JsonElement> Descendants(this JsonElement element)
    {
        yield return element;

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    foreach (var child in property.Value.Descendants())
                    {
                        yield return child;
                    }
                }

                break;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    foreach (var child in item.Descendants())
                    {
                        yield return child;
                    }
                }

                break;
        }
    }

    public static JsonElement? GetPropertyOrNull(this JsonElement element, string name)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value))
        {
            return value;
        }

        return null;
    }

    public static string? GetStringOrNull(this JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.ToString(),
            _ => null
        };
    }

    public static string? ReadRunsText(this JsonElement? element)
    {
        if (element is null)
        {
            return null;
        }

        var node = element.Value;
        if (node.ValueKind == JsonValueKind.String)
        {
            return node.GetString();
        }

        if (node.TryGetProperty("simpleText", out var simple))
        {
            return simple.GetString();
        }

        if (node.TryGetProperty("runs", out var runs) && runs.ValueKind == JsonValueKind.Array)
        {
            return string.Concat(runs.EnumerateArray().Select(r => r.GetPropertyOrNull("text")?.GetString() ?? string.Empty));
        }

        return null;
    }

    public static IReadOnlyList<(string Text, string? PageType, string? BrowseId, string? VideoId)> ReadRuns(
        this JsonElement? element)
    {
        if (element is null || element.Value.ValueKind != JsonValueKind.Object)
        {
            return [];
        }

        if (!element.Value.TryGetProperty("runs", out var runs) || runs.ValueKind != JsonValueKind.Array)
        {
            var simple = element.ReadRunsText();
            return string.IsNullOrEmpty(simple) ? [] : [(simple, null, null, null)];
        }

        var list = new List<(string, string?, string?, string?)>();
        foreach (var run in runs.EnumerateArray())
        {
            var text = run.GetPropertyOrNull("text")?.GetString() ?? string.Empty;
            string? pageType = null;
            string? browseId = null;
            string? videoId = null;

            var nav = run.GetPropertyOrNull("navigationEndpoint");
            var browse = nav?.GetPropertyOrNull("browseEndpoint");
            if (browse is not null)
            {
                browseId = browse.Value.GetPropertyOrNull("browseId")?.GetString();
                pageType = browse.Value
                    .GetPropertyOrNull("browseEndpointContextSupportedConfigs")
                    ?.GetPropertyOrNull("browseEndpointContextMusicConfig")
                    ?.GetPropertyOrNull("pageType")
                    ?.GetString();
            }

            videoId = nav?.GetPropertyOrNull("watchEndpoint")?.GetPropertyOrNull("videoId")?.GetString();
            list.Add((text, pageType, browseId, videoId));
        }

        return list;
    }
}
