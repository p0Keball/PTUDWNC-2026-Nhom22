using FoodBlog.Domain.Common;
using FoodBlog.Domain.Enums;

namespace FoodBlog.Domain.Entities;

public class Recipe : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Servings { get; set; }
    public Difficulty Difficulty { get; set; } = Difficulty.Easy;
    public RecipeStatus Status { get; set; } = RecipeStatus.Draft;
    public Guid CategoryId { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }

    public RecipeNutrition? Nutrition { get; set; }
    public Category? Category { get; set; }
    public ApplicationUser? Author { get; set; }
    public ICollection<RecipeStep> Steps { get; set; } = new List<RecipeStep>();
    public ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
    public ICollection<RecipeImage> Images { get; set; } = new List<RecipeImage>();
}
