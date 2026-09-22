using FoodBlog.Domain.Common;

namespace FoodBlog.Domain.Entities;

/// <summary>
/// Danh mục công thức (SRS §7.6: Categories).
/// Private setters + factory method đảm bảo entity luôn hợp lệ (DDD).
/// </summary>
public class Category : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public int OrderIndex { get; private set; }

    // Navigation: 1 Category -> N Recipe (Restrict Delete, FR-CAT-005)
    public ICollection<Recipe> Recipes { get; private set; } = [];

    // EF Core cần constructor không tham số
    protected Category() { }

    // Factory method — cách DUY NHẤT để tạo Category hợp lệ
    public static Category Create(string name, string? description = null, string? imageUrl = null, int orderIndex = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Slug = SlugHelper.Generate(name),
            Description = description?.Trim(),
            ImageUrl = imageUrl?.Trim(),
            OrderIndex = orderIndex,
            CreatedAt = DateTime.UtcNow,
        };
    }

    // Business method — đóng gói logic update (Slug KHÔNG đổi khi đổi tên, FR-CAT-004)
    public void Update(string name, string? description, string? imageUrl = null, int? orderIndex = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        Name = name.Trim();
        // Giữ slug bất biến để tránh broken links (FR-CAT-004, xem FAILURE.md F-14)
        Description = description?.Trim();
        if (imageUrl is not null) ImageUrl = imageUrl.Trim();
        if (orderIndex.HasValue) OrderIndex = orderIndex.Value;
        UpdatedAt = DateTime.UtcNow;
    }
}
