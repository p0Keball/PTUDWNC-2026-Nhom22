using FoodBlog.Domain.Common;

namespace FoodBlog.Domain.Entities;

/// <summary>
/// Bước thực hiện công thức (SRS §7.3: RecipeSteps).
/// Skeleton — thành viên 4 bổ sung chi tiết (Tuần 1).
/// </summary>
public class RecipeStep : BaseEntity
{
    public Guid RecipeId { get; private set; }
    public int StepNumber { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int? TimerMinutes { get; private set; }
    public string? ImageUrl { get; private set; }

    // Navigation
    public Recipe? Recipe { get; private set; }

    protected RecipeStep() { }

    public static RecipeStep Create(
        Guid recipeId,
        int stepNumber,
        string title,
        string description,
        int? timerMinutes = null,
        string? imageUrl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));

        return new RecipeStep
        {
            Id = Guid.NewGuid(),
            RecipeId = recipeId,
            StepNumber = stepNumber,
            Title = title.Trim(),
            Description = description.Trim(),
            TimerMinutes = timerMinutes,
            ImageUrl = imageUrl,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
