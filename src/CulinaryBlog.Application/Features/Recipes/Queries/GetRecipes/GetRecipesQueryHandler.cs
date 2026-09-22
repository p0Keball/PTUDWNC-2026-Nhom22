using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Contracts.Persistence;
using CulinaryBlog.Application.DTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Application.Features.Recipes.Queries.GetRecipes;

public class GetRecipesQueryHandler : IRequestHandler<GetRecipesQuery, PaginatedResult<RecipeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRecipesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PaginatedResult<RecipeDto>> Handle(GetRecipesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Recipes.AsNoTracking();

        // Filtering
        if (request.CategoryId.HasValue)
            query = query.Where(r => r.CategoryId == request.CategoryId.Value);

        if (request.Difficulty.HasValue)
            query = query.Where(r => r.Difficulty == request.Difficulty.Value);

        if (request.MinCookTime.HasValue)
            query = query.Where(r => r.CookTimeMinutes >= request.MinCookTime.Value);

        if (request.MaxCookTime.HasValue)
            query = query.Where(r => r.CookTimeMinutes <= request.MaxCookTime.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(r => r.Title.ToLower().Contains(search) || r.Description.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Sorting
        query = (request.SortBy.ToLower(), request.Descending) switch
        {
            ("cooktime", false) => query.OrderBy(r => r.CookTimeMinutes),
            ("cooktime", true) => query.OrderByDescending(r => r.CookTimeMinutes),
            ("createdat", false) => query.OrderBy(r => r.CreatedAt),
            ("createdat", true) => query.OrderByDescending(r => r.CreatedAt),
            _ => request.Descending ? query.OrderByDescending(r => r.Title) : query.OrderBy(r => r.Title)
        };

        // Pagination & Projection
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectToType<RecipeDto>()
            .ToListAsync(cancellationToken);

        return new PaginatedResult<RecipeDto>(items, totalCount, request.Page, request.PageSize);
    }
}
