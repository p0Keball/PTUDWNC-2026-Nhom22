namespace FoodBlog.Application.Common.Interfaces;

/// <summary>
/// Ghi nhật ký hệ thống (audit trail) cho các thao tác ghi.
/// Triển khai phải "best-effort": lỗi ghi log không được làm hỏng nghiệp vụ.
/// </summary>
public interface IAuditLogger
{
    Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
}

/// <param name="Action">Mã hành động, ví dụ RECIPE_PUBLISHED.</param>
/// <param name="EntityType">Loại đối tượng: Recipe, RecipeImage, User, Auth...</param>
/// <param name="EntityId">Id đối tượng (nếu có).</param>
/// <param name="Description">Mô tả người đọc được.</param>
/// <param name="Succeeded">Thao tác thành công hay thất bại.</param>
/// <param name="UserId">Dùng khi chưa có ClaimsPrincipal (ví dụ login/register).</param>
/// <param name="UserEmail">Dùng khi chưa có ClaimsPrincipal.</param>
public sealed record AuditLogEntry(
    string Action,
    string EntityType,
    string? EntityId,
    string Description,
    bool Succeeded = true,
    string? UserId = null,
    string? UserEmail = null);
