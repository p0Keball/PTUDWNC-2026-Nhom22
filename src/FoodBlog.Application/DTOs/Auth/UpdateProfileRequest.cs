namespace FoodBlog.Application.DTOs.Auth;

public sealed record UpdateProfileRequest(
    string? DisplayName,
    string? AvatarUrl,
    string? Bio);
