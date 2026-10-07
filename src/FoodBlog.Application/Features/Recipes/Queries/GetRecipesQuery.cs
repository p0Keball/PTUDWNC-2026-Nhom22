using FluentValidation;
using FoodBlog.Application.Common.Interfaces;
using FoodBlog.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.Application.Features.Recipes.Queries;

public record GetRecipesQuery(
    int Page = 1,
    int PageSize = 12,
    Guid? CategoryId = null,
    int? Difficulty = null,
    int? MaxCookTime = null,
    string Sort = "-createdAt") : IRequest<PagedResult<RecipeSummaryDto>>;

public sealed class GetRecipesQueryValidator : AbstractValidator<GetRecipesQuery>
{
    public GetRecipesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}

public sealed class GetRecipesQueryHandler(IFoodBlogDbContext db) : IRequestHandler<GetRecipesQuery, PagedResult<RecipeSummaryDto>>
{
    public async Task<PagedResult<RecipeSummaryDto>> Handle(GetRecipesQuery req, CancellationToken ct)
    {
        var page = Math.Max(req.Page, 1);
        var pageSize = Math.Clamp(req.PageSize, 1, 50);

        var query = db.Recipes.Where(r => r.Status == RecipeStatus.Published);
        if (req.CategoryId.HasValue) query = query.Where(r => r.CategoryId == req.CategoryId);
        if (req.Difficulty.HasValue) query = query.Where(r => r.Difficulty == (Difficulty)req.Difficulty);
        if (req.MaxCookTime.HasValue) query = query.Where(r => r.CookTimeMinutes <= req.MaxCookTime);
        query = req.Sort switch
        {
            "createdAt" => query.OrderBy(r => r.CreatedAt),
            "title" => query.OrderBy(r => r.Title),
            "-title" => query.OrderByDescending(r => r.Title),
            _ => query.OrderByDescending(r => r.CreatedAt)
        };

        var total = await query.CountAsync(ct);
        var items = await query
            .AsNoTracking()
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => new RecipeSummaryDto(r.Id, r.Title, r.Slug, r.Description,
                r.Images.Where(i => i.IsPrimary).Select(i => i.OriginalUrl).FirstOrDefault(),
                r.PrepTimeMinutes, r.CookTimeMinutes, r.Servings,
                r.Difficulty.ToString(), r.PublishedAt))
            .ToListAsync(ct);

        return new PagedResult<RecipeSummaryDto>(items, total, page, pageSize);
    }
}
