namespace SmartTaskManagement.Application.Common;

/// <summary>
/// A generic paged result returned by server-side list queries.
/// </summary>
/// <typeparam name="T">The type of the items in the current page.</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>Items on the current page.</summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>Total number of records across all pages.</summary>
    public int TotalCount { get; }

    /// <summary>Current page number (1-based).</summary>
    public int PageNumber { get; }

    /// <summary>Page size requested.</summary>
    public int PageSize { get; }

    /// <summary>Total pages available.</summary>
    public int TotalPages { get; }

    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}
