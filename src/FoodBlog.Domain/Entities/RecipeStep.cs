using System;
using FoodBlog.Domain.Common;

namespace FoodBlog.Domain.Entities;

public class RecipeStep : BaseEntity
{
    public Guid RecipeId { get; private set; }
    public int StepNumber { get; private set; }
    public string Title { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public int? TimerMinutes { get; private set; }
    public string? ImageUrl { get; private set; }

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

    public void Renumber(int stepNumber)
    {
        StepNumber = stepNumber;
    }
}