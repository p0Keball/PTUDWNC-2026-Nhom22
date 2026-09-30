using FoodBlog.Domain.Common;

namespace FoodBlog.Domain.Entities;

public class RecipeImage : BaseEntity
{
    public Guid RecipeId { get; set; }
    public string OriginalUrl { get; set; } = string.Empty;
    public string? MediumUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; }
    public int OrderIndex { get; set; }

<<<<<<< Updated upstream
    public Recipe? Recipe { get; set; }
}
=======
    // Navigation property
    public Recipe Recipe { get; private set; } = default!;
    
    // Constructor mặc định cho EF Core
    private RecipeImage() { }

    // Factory Method để khởi tạo entity
    public static RecipeImage Create(Guid recipeId, string originalUrl, string? mediumUrl, string? thumbnailUrl, string? altText, bool isPrimary, int orderIndex)
    {
        return new RecipeImage
        {
            RecipeId = recipeId,
            OriginalUrl = originalUrl,
            MediumUrl = mediumUrl,
            ThumbnailUrl = thumbnailUrl,
            AltText = altText,
            IsPrimary = isPrimary,
            OrderIndex = orderIndex
        };
    }

    public void SetPrimary(bool isPrimary) => IsPrimary = isPrimary;
}
>>>>>>> Stashed changes
