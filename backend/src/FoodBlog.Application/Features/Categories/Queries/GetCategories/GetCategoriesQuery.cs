using FoodBlog.Application.Common.Models;
using FoodBlog.Application.DTOs;
using MediatR;

namespace FoodBlog.Application.Features.Categories.Queries.GetCategories;

/// <summary>
/// Query danh sách phân trang + search + sort (§1.3.5).
/// </summary>
public record GetCategoriesQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    string SortBy = "name",
    bool Descending = false
) : IRequest<PaginatedResult<CategoryDto>>;
