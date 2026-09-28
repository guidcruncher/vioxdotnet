using System.Text.Json.Serialization;

using Viox.Client.Podverse.Converters;

namespace Viox.Client.Podverse.Models;

/// <summary>
/// Represents a paginated tuple payload formatted as [ items[], totalCount ].
/// </summary>
/// <typeparam name="T">The type of items contained within the response array.</typeparam>
[JsonConverter(typeof(PodversePagedResultConverterFactory))]
public sealed class PodversePagedResult<T>
{
    /// <summary>
    /// Gets or sets the list of items returned for the requested page.
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the total count of items available across all pages.
    /// </summary>
    public int TotalCount { get; set; }
}
