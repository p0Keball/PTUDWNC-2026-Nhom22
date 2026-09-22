using CulinaryBlog.Application.DTOs;
using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Commands.CreateRecipe;

public record CreateRecipeCommand(
    string Title,
    string Description,
    string Instructions,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    Difficulty Difficulty,
    Guid CategoryId,
    Guid AuthorId
) : IRequest<RecipeDto>;
