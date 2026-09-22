using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs;
using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Queries.GetRecipes;

public record GetRecipesQuery(
    Guid? CategoryId = null,
    Difficulty? Difficulty = null,
    int? MinCookTime = null,
    int? MaxCookTime = null,
    string? Search = null,
    string SortBy = "title",
    bool Descending = false,
    int Page = 1,
    int PageSize = 10
) : IRequest<PaginatedResult<RecipeDto>>;
