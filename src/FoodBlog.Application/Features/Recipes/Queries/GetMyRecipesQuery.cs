using FluentValidation;
using FoodBlog.Application.Common.Interfaces;
using FoodBlog.Domain.Enums;
using FoodBlog.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.Application.Features.Recipes.Queries;

public record GetMyRecipesQuery(
    int Page = 1,
    int PageSize = 12,
    string? Status = null) : IRequest<PagedResult<RecipeSummaryDto>>;

public sealed class GetMyRecipesQueryValidator : AbstractValidator<GetMyRecipesQuery>
{
    private static readonly string[] AllowedStatuses = ["draft", "published", "archived"];

    public GetMyRecipesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
        RuleFor(x => x.Status)
            .Must(s => s is null || AllowedStatuses.Contains(s.Trim().ToLowerInvariant()))
            .WithMessage("Trạng thái phải là draft, published hoặc archived.");
    }
}

public sealed class GetMyRecipesQueryHandler(IFoodBlogDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GetMyRecipesQuery, PagedResult<RecipeSummaryDto>>
{
    public async Task<PagedResult<RecipeSummaryDto>> Handle(GetMyRecipesQuery req, CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            throw new RecipeUnauthorizedException();

        var page = Math.Max(req.Page, 1);
        var pageSize = Math.Clamp(req.PageSize, 1, 50);

        var query = db.Recipes.Where(r => r.AuthorId == currentUser.UserId);

        if (!string.IsNullOrWhiteSpace(req.Status)
            && Enum.TryParse<RecipeStatus>(req.Status.Trim(), ignoreCase: true, out var status))
            query = query.Where(r => r.Status == status);

        query = query.OrderByDescending(r => r.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .AsNoTracking()
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => new RecipeSummaryDto(r.Id, r.Title, r.Slug, r.Description,
                r.Images.Where(i => i.IsPrimary).Select(i => i.OriginalUrl).FirstOrDefault(),
                r.PrepTimeMinutes, r.CookTimeMinutes, r.Servings,
                r.Difficulty.ToString(), r.Status.ToString(), r.PublishedAt))
            .ToListAsync(ct);

        return new PagedResult<RecipeSummaryDto>(items, total, page, pageSize);
    }
}
