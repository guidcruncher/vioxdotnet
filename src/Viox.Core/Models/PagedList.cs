namespace Viox.Core.Models;

/// <summary>
/// Represents a paginated subset of items using offset and limit parameters.
/// </summary>
/// <typeparam name="T">The type of elements in the paged list.</typeparam>
public class PagedList<T> : List<T>
{
    /// <summary>
    /// Gets the starting index (0-based) of the paged subset.
    /// </summary>
    public int Offset { get; }

    public int PageNumber { get => (Offset == 0 ? 0 : Offset / Limit); }

    /// <summary>
    /// Gets the maximum number of items requested for the page.
    /// </summary>
    public int Limit { get; }

    /// <summary>
    /// Gets the total count of items in the unpaged source collection.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// Gets a value indicating whether there is a previous page available.
    /// </summary>
    public bool HasPreviousPage => Offset > 0;

    /// <summary>
    /// Gets a value indicating whether there is a next page available.
    /// </summary>
    public bool HasNextPage => Offset + Limit < TotalCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="PagedList{T}"/> class.
    /// </summary>
    /// <param name="items">The items belonging to the current page.</param>
    /// <param name="totalCount">The total count of items across all pages.</param>
    /// <param name="offset">The 0-based offset starting index.</param>
    /// <param name="limit">The maximum number of items per page.</param>
    public PagedList(IEnumerable<T> items, int totalCount, int offset, int limit)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);

        Offset = offset;
        Limit = limit;
        TotalCount = totalCount;

        AddRange(items);
    }

    public PagedList()
    {
        Offset = 0;
        Limit = 0;
        TotalCount = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PagedList{T}"/> class.
    /// </summary>
    /// <param name="items">The full result set.</param>
    /// <param name="offset">The 0-based offset starting index.</param>
    /// <param name="limit">The maximum number of items per page.</param>
    public PagedList(IEnumerable<T> items, int offset, int limit)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);
        Offset = offset;
        Limit = limit;
        TotalCount = items.Count();
        AddRange(items.Skip(offset).Take(limit).ToList());
    }

    /// <summary>
    /// Creates a <see cref="PagedList{T}"/> from an <see cref="IEnumerable{T}"/> source by applying offset and limit parameters.
    /// </summary>
    /// <param name="source">The underlying source collection.</param>
    /// <param name="offset">The 0-based offset indicating where to begin taking items.</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <returns>A new instance of <see cref="PagedList{T}"/> containing the paged subset.</returns>
    public static PagedList<T> Create(IEnumerable<T> source, int offset, int limit)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1);

        int totalCount = source.Count();
        List<T> items = source.Skip(offset).Take(limit).ToList();

        return new PagedList<T>(items, totalCount, offset, limit);
    }
}
