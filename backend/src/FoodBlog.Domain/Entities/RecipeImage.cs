using FoodBlog.Domain.Common;

namespace FoodBlog.Domain.Entities;

/// <summary>
/// Ảnh công thức (SRS §7.5: RecipeImages).
/// QUYẾT ĐỊNH (FAILURE F-10): khi xóa ảnh đang IsPrimary, handler phải
/// reassign primary cho ảnh còn lại (OrderIndex nhỏ nhất); MediumUrl/
/// ThumbnailUrl do Hangfire job điền sau (FR-JOB-002), NULL tạm thời.
/// </summary>
public class RecipeImage : BaseEntity
{
    public Guid RecipeId { get; private set; }
    public string OriginalUrl { get; private set; } = string.Empty;
    public string? MediumUrl { get; private set; }
    public string? ThumbnailUrl { get; private set; }
    public string? AltText { get; private set; }
    public bool IsPrimary { get; private set; }
    public int OrderIndex { get; private set; }

    // Navigation
    public Recipe? Recipe { get; private set; }

    protected RecipeImage() { }

    public static RecipeImage Create(
        Guid recipeId,
        string originalUrl,
        string? altText = null,
        bool isPrimary = false,
        int orderIndex = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(originalUrl, nameof(originalUrl));

        return new RecipeImage
        {
            Id = Guid.NewGuid(),
            RecipeId = recipeId,
            OriginalUrl = originalUrl.Trim(),
            AltText = altText?.Trim(),
            IsPrimary = isPrimary,
            OrderIndex = orderIndex,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetVariants(string? mediumUrl, string? thumbnailUrl)
    {
        MediumUrl = mediumUrl;
        ThumbnailUrl = thumbnailUrl;
        UpdatedAt = DateTime.UtcNow;
    }
}
