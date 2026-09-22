using FoodBlog.Domain.Common;

namespace FoodBlog.Domain.Entities;

/// <summary>
/// Nguyên liệu công thức (SRS §7.4: RecipeIngredients).
/// QUYẾT ĐỊNH (FAILURE F-12): thống nhất OrderIndex (không dùng SortOrder).
/// </summary>
public class RecipeIngredient : BaseEntity
{
    public Guid RecipeId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal? Quantity { get; private set; }
    public string? Unit { get; private set; }
    public string? Notes { get; private set; }
    public int OrderIndex { get; private set; }

    // Navigation
    public Recipe? Recipe { get; private set; }

    protected RecipeIngredient() { }

    public static RecipeIngredient Create(
        Guid recipeId,
        string name,
        decimal? quantity = null,
        string? unit = null,
        string? notes = null,
        int orderIndex = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        return new RecipeIngredient
        {
            Id = Guid.NewGuid(),
            RecipeId = recipeId,
            Name = name.Trim(),
            Quantity = quantity,
            Unit = unit?.Trim(),
            Notes = notes?.Trim(),
            OrderIndex = orderIndex,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
