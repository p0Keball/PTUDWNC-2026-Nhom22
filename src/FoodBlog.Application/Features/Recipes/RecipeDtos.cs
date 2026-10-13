using FoodBlog.Domain.Entities;

namespace FoodBlog.Application.Features.Recipes;

public record NutritionDto(decimal? Calories, decimal? Protein, decimal? Carbohydrates,
    decimal? Fat, decimal? Fiber, decimal? Sodium);
public record RecipeStepDto(Guid Id, int StepNumber, string Title, string Description, int? TimerMinutes, string? ImageUrl);
public record RecipeIngredientDto(Guid Id, string Name, decimal? Quantity, string? Unit, string? Notes, int OrderIndex);
public record RecipeImageDto(Guid Id, string OriginalUrl, string? MediumUrl, string? ThumbnailUrl,
    string? AltText, bool IsPrimary, int OrderIndex, string ThumbnailStatus);
public record RecipeDetailDto(Guid Id, string Title, string Slug, string Description, string Instructions,
    int PrepTimeMinutes, int CookTimeMinutes, int Servings, string Difficulty, string Status,
    Guid CategoryId, string AuthorId, DateTime? PublishedAt, string RowVersion,
    NutritionDto? Nutrition, List<RecipeStepDto> Steps, List<RecipeIngredientDto> Ingredients, List<RecipeImageDto> Images);
public record RecipeSummaryDto(Guid Id, string Title, string Slug, string Description,
    string? PrimaryImageUrl, int PrepTimeMinutes, int CookTimeMinutes, int Servings,
    string Difficulty, DateTime? PublishedAt);
public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);

public static class RecipeMapper
{
    public static RecipeDetailDto ToDetail(Recipe r) => new(
        r.Id, r.Title, r.Slug, r.Description, r.Instructions,
        r.PrepTimeMinutes, r.CookTimeMinutes, r.Servings,
        r.Difficulty.ToString(), r.Status.ToString(),
        r.CategoryId, r.AuthorId, r.PublishedAt,
        Convert.ToBase64String(r.RowVersion ?? []),
        r.Nutrition is null ? null : new NutritionDto(r.Nutrition.Calories, r.Nutrition.Protein,
            r.Nutrition.Carbohydrates, r.Nutrition.Fat, r.Nutrition.Fiber, r.Nutrition.Sodium),
        r.Steps.OrderBy(s => s.StepNumber)
            .Select(s => new RecipeStepDto(s.Id, s.StepNumber, s.Title, s.Description, s.TimerMinutes, s.ImageUrl)).ToList(),
        r.Ingredients.OrderBy(i => i.OrderIndex)
            .Select(i => new RecipeIngredientDto(i.Id, i.Name, i.Quantity, i.Unit, i.Notes, i.OrderIndex)).ToList(),
        r.Images.OrderBy(i => i.OrderIndex)
            .Select(i => new RecipeImageDto(i.Id, i.OriginalUrl, i.MediumUrl, i.ThumbnailUrl, i.AltText, i.IsPrimary, i.OrderIndex,
                i.ThumbnailUrl is null && i.MediumUrl is null ? "pending" : "ready")).ToList());

    public static RecipeSummaryDto ToSummary(Recipe r, string? primaryImageUrl) => new(
        r.Id, r.Title, r.Slug, r.Description, primaryImageUrl,
        r.PrepTimeMinutes, r.CookTimeMinutes, r.Servings,
        r.Difficulty.ToString(), r.PublishedAt);
}
