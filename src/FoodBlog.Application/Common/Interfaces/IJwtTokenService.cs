using FoodBlog.Domain.Entities;

namespace FoodBlog.Application.Common.Interfaces;

/// <summary>
/// Sinh JWT access token (HS256) cho người dùng đã xác thực.
/// </summary>
public interface IJwtTokenService
{
    (string AccessToken, DateTime ExpiresAt) CreateAccessToken(
        ApplicationUser user,
        IEnumerable<string> roles);
}
