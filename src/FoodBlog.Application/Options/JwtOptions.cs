namespace FoodBlog.Application.Options;

/// <summary>
/// Cấu hình JWT theo NFR-SEC-002: access token HS256 15 phút,
/// refresh token 7 ngày (rotation + reuse detection).
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "FoodBlog.API";
    public string Audience { get; set; } = "FoodBlog.Client";
    public string Secret { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
