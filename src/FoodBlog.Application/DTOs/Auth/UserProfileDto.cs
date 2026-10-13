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
    DateTime CreatedAt);
