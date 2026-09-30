using FoodBlog.Domain.Common;

namespace FoodBlog.Domain.Entities;

public class RecipeIngredient : BaseEntity
{
    public Guid RecipeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Notes { get; set; }
    public int OrderIndex { get; set; }

<<<<<<< Updated upstream
    public Recipe? Recipe { get; set; }
}
=======
    // Navigation property
    public Recipe Recipe { get; private set; } = default!;

    // Constructor mặc định cho EF Core
    private RecipeIngredient() { }

    // Factory Method để khởi tạo entity
    public static RecipeIngredient Create(Guid recipeId, string name, decimal? quantity, string? unit, string? notes, int orderIndex)
    {
        return new RecipeIngredient
        {
            RecipeId = recipeId,
            Name = name,
            Quantity = quantity,
            Unit = unit,
            Notes = notes,
            OrderIndex = orderIndex
        };
    }

    public void Update(string name, decimal? quantity, string? unit, string? notes)
    {
        Name = name;
        Quantity = quantity;
        Unit = unit;
        Notes = notes;
    }
}
>>>>>>> Stashed changes
