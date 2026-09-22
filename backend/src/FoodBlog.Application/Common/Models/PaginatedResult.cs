namespace FoodBlog.Application.Common.Models;

/// <summary>
/// Generic wrapper cho mọi kết quả phân trang (§1.3.5).
/// Chứa đủ metadata để client tự xây navigation links.
/// </summary>
public sealed class PaginatedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    public PaginatedResult(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }
}
