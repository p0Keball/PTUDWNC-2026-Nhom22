using System;
using FoodBlog.Domain.Common;

namespace FoodBlog.Domain.Entities;

public class RecipeImage : BaseEntity
{
    public Guid RecipeId { get; private set; }
    public string OriginalUrl { get; private set; } = default!;
    public string? MediumUrl { get; private set; }
    public string? ThumbnailUrl { get; private set; }
    public string? AltText { get; private set; }
    public bool IsPrimary { get; private set; }
    public int OrderIndex { get; private set; }

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
}