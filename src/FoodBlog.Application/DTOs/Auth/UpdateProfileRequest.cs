namespace FoodBlog.Application.DTOs.Auth;

/// <summary>Dữ liệu cập nhật từng phần cho hồ sơ người dùng.</summary>
public sealed record UpdateProfileRequest(
    string? DisplayName,
    string? AvatarUrl,
    string? Bio);
