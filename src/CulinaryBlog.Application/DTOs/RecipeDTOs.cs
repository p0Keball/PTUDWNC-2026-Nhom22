using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.Application.DTOs;

public record RecipeDto(
    Guid Id,
    string Title,
    string Description,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    Difficulty Difficulty,
    Guid CategoryId,
    DateTime CreatedAt
);

public record RecipeDetailDto(
    Guid Id,
    string Title,
    string Description,
    string Instructions,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    Difficulty Difficulty,
    Guid CategoryId,
    string CategoryName,
    Guid AuthorId,
    DateTime CreatedAt
);
