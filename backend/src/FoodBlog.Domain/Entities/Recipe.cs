using FoodBlog.Domain.Common;

namespace FoodBlog.Domain.Entities;

/// <summary>
/// Độ khó công thức (SRS §7.2 Difficulty: 1=Easy, 2=Medium, 3=Hard, 4=Expert).
/// </summary>
public enum DifficultyLevel : short
{
    Easy = 1,
    Medium = 2,
    Hard = 3,
    Expert = 4,
}

/// <summary>
/// Trạng thái công thức (SRS §7.2 Status: 0=Draft, 1=Published, 2=Archived).
/// </summary>
public enum RecipeStatus : short
{
    Draft = 0,
    Published = 1,
    Archived = 2,
}

/// <summary>
/// Công thức nấu ăn (SRS §7.2: Recipes).
/// Skeleton Chương 1 — chi tiết Steps/Ingredients/Images/Nutrition do các thành viên bổ sung (Tuần 1).
/// </summary>
public class Recipe : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Instructions { get; private set; } = string.Empty;
    public int PrepTimeMinutes { get; private set; }
    public int CookTimeMinutes { get; private set; }
    public int Servings { get; private set; }
    public DifficultyLevel Difficulty { get; private set; } = DifficultyLevel.Easy;
    public RecipeStatus Status { get; private set; } = RecipeStatus.Draft;
    public Guid CategoryId { get; private set; }
    public string AuthorId { get; private set; } = string.Empty;
    public DateTime? PublishedAt { get; private set; }

    // Navigation
    public Category? Category { get; private set; }
    public ApplicationUser? Author { get; private set; }
    public RecipeNutrition? Nutrition { get; private set; }
    public ICollection<RecipeStep> Steps { get; private set; } = [];
    public ICollection<RecipeIngredient> Ingredients { get; private set; } = [];
    public ICollection<RecipeImage> Images { get; private set; } = [];

    protected Recipe() { }

    public static Recipe Create(
        string title,
        string description,
        Guid categoryId,
        string authorId,
        int prepTimeMinutes,
        int cookTimeMinutes,
        int servings,
        DifficultyLevel difficulty = DifficultyLevel.Easy,
        string instructions = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));

        return new Recipe
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Slug = SlugHelper.Generate(title),
            Description = description.Trim(),
            Instructions = instructions,
            CategoryId = categoryId,
            AuthorId = authorId,
            PrepTimeMinutes = prepTimeMinutes,
            CookTimeMinutes = cookTimeMinutes,
            Servings = servings,
            Difficulty = difficulty,
            Status = RecipeStatus.Draft,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Publish()
    {
        // Business rule FR-RCP-005: không publish khi thiếu Steps
        if (Steps.Count == 0)
            throw new Exceptions.DomainException(
                "Công thức phải có ít nhất 1 bước thực hiện mới được xuất bản.",
                "RECIPE_PUBLISH_INCOMPLETE");

        Status = RecipeStatus.Published;
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unpublish()
    {
        Status = RecipeStatus.Draft;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetNutrition(RecipeNutrition? nutrition)
    {
        Nutrition = nutrition;
        UpdatedAt = DateTime.UtcNow;
    }
}
