namespace FoodBlog.Domain.Entities;

/// <summary>
/// Nhật ký hệ thống (audit trail) — ghi lại mọi thao tác ghi quan trọng.
/// Phục vụ NFR-SEC-006 (audit log) và yêu cầu "nhật ký hệ thống" của đồ án.
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Id người thực hiện (null với thao tác ẩn danh như đăng nhập thất bại).</summary>
    public string? UserId { get; set; }

    public string? UserEmail { get; set; }

    /// <summary>Mã hành động, ví dụ RECIPE_PUBLISHED, IMAGE_UPLOADED, USER_DEACTIVATED.</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Loại đối tượng bị tác động: Recipe, RecipeImage, User, Auth...</summary>
    public string EntityType { get; set; } = string.Empty;

    public string? EntityId { get; set; }

    /// <summary>Mô tả người đọc được, ví dụ: Xuất bản công thức "Phở bò".</summary>
    public string Description { get; set; } = string.Empty;

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    /// <summary>CorrelationId của HTTP request để đối chiếu với log Serilog/Seq.</summary>
    public string? CorrelationId { get; set; }

    public bool Succeeded { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
