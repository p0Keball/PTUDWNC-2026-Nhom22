using FoodBlog.Application.Common.Models;
using FoodBlog.Application.Contracts.Persistence;
using FoodBlog.Application.DTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.Application.Features.Categories.Queries.GetCategories;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, PaginatedResult<CategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        // AsNoTracking: chỉ đọc (Query, không phải Command)
        var query = _context.Categories.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower().Trim();
            query = query.Where(c =>
                c.Name.ToLower().Contains(search) ||
                (c.Description != null && c.Description.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = (request.SortBy.ToLower(), request.Descending) switch
        {
            ("name", false) => query.OrderBy(c => c.Name),
            ("name", true) => query.OrderByDescending(c => c.Name),
            ("createdat", false) => query.OrderBy(c => c.CreatedAt),
            ("createdat", true) => query.OrderByDescending(c => c.CreatedAt),
            _ => query.OrderBy(c => c.Name)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToType<CategoryDto>()
            .ToListAsync(cancellationToken);

        return new PaginatedResult<CategoryDto>(items, totalCount, request.Page, request.PageSize);
    }
}
