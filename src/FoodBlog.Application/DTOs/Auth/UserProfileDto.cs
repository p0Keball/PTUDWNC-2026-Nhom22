namespace FoodBlog.Application.DTOs.Auth;

/// <summary>Thông tin hồ sơ của người dùng đang đăng nhập.</summary>
public sealed record UserProfileDto(
    string Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    string? Bio,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    // FR-AUTH-006: trạng thái xác nhận email phục vụ policy VerifiedAuthor.
    bool EmailConfirmed);
