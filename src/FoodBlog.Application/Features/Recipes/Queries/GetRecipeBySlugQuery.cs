using FluentValidation;
using FoodBlog.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.Application.Features.Recipes.Queries;

public record GetRecipeBySlugQuery(string Slug) : IRequest<RecipeDetailDto?>;

public sealed class GetRecipeBySlugQueryValidator : AbstractValidator<GetRecipeBySlugQuery>
{
    public GetRecipeBySlugQueryValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().WithMessage("Slug không được để trống.");
    }
}

public sealed class GetRecipeBySlugQueryHandler(IFoodBlogDbContext db) : IRequestHandler<GetRecipeBySlugQuery, RecipeDetailDto?>
{
    public async Task<RecipeDetailDto?> Handle(GetRecipeBySlugQuery req, CancellationToken ct)
    {
        var recipe = await db.Recipes
            .Include(r => r.Steps)
            .Include(r => r.Ingredients)
            .Include(r => r.Images)
            .FirstOrDefaultAsync(r => r.Slug == req.Slug, ct);

        return recipe is null ? null : RecipeMapper.ToDetail(recipe);
    }
}
