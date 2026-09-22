using CulinaryBlog.Application.Contracts.Persistence;
using CulinaryBlog.Application.DTOs;
using CulinaryBlog.Domain.Entities;
using Mapster;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Commands.CreateRecipe;

public class CreateRecipeCommandHandler : IRequestHandler<CreateRecipeCommand, RecipeDto>
{
    private readonly IApplicationDbContext _context;

    public CreateRecipeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RecipeDto> Handle(CreateRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = Recipe.Create(
            request.Title, request.Description, request.Instructions,
            request.PrepTimeMinutes, request.CookTimeMinutes, request.Servings,
            request.Difficulty, request.CategoryId, request.AuthorId);

        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync(cancellationToken);

        return recipe.Adapt<RecipeDto>();
    }
}
