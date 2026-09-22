using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.Domain.Entities;

public class Recipe
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Instructions { get; private set; } = string.Empty;
    public int PrepTimeMinutes { get; private set; }
    public int CookTimeMinutes { get; private set; }
    public int Servings { get; private set; }
    public Difficulty Difficulty { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public Guid AuthorId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected Recipe() { }

    public static Recipe Create(
        string title, string description, string instructions,
        int prepTime, int cookTime, int servings, Difficulty difficulty,
        Guid categoryId, Guid authorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));
        
        return new Recipe
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Description = description.Trim(),
            Instructions = instructions.Trim(),
            PrepTimeMinutes = prepTime,
            CookTimeMinutes = cookTime,
            Servings = servings,
            Difficulty = difficulty,
            CategoryId = categoryId,
            AuthorId = authorId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
