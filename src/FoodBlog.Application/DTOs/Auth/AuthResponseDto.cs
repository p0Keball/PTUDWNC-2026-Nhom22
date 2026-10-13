namespace FoodBlog.Application.DTOs.Auth;

public sealed record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

public sealed record UserDto(
    string Id,
    string Email,
    string FullName,
    string Role
);