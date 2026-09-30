using FoodBlog.Domain.Common;

namespace FoodBlog.Domain.Entities;

public class RecipeStep : BaseEntity
{
    public Guid RecipeId { get; set; }
    public int StepNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? TimerMinutes { get; set; }
    public string? ImageUrl { get; set; }

<<<<<<< Updated upstream
    public Recipe? Recipe { get; set; }
}
=======
    // Navigation property
    public Recipe Recipe { get; private set; } = default!;

    // Constructor mặc định bắt buộc cho EF Core (đặt private để bảo vệ tính toàn vẹn của Domain)
    private RecipeStep() { }

    // Factory Method để khởi tạo entity
    public static RecipeStep Create(Guid recipeId, int stepNumber, string title, string description, int? timerMinutes, string? imageUrl)
    {
        return new RecipeStep
        {
            RecipeId = recipeId,
            StepNumber = stepNumber,
            Title = title,
            Description = description,
            TimerMinutes = timerMinutes,
            ImageUrl = imageUrl
        };
    }

    public void Update(string title, string description, int? timerMinutes, string? imageUrl)
    {
        Title = title;
        Description = description;
        TimerMinutes = timerMinutes;
        ImageUrl = imageUrl;
    }

    public void SetStepNumber(int stepNumber) => StepNumber = stepNumber;
}
>>>>>>> Stashed changes
