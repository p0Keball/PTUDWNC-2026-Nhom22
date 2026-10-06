namespace FoodBlog.Application.DTOs.Auth;

public sealed record UserProfileDto(
    string Id,
    string Email,
    string DisplayName,
    string? AvatarUrl,
    string? Bio,
    string Role,
    bool IsActive,
    DateTime CreatedAt);
