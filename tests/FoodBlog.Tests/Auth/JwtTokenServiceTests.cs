using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FoodBlog.Application.Options;
using FoodBlog.Domain.Entities;
using FoodBlog.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace FoodBlog.Tests.Auth;

public sealed class JwtTokenServiceTests
{
    private static JwtTokenService CreateService(int accessMinutes = 15) =>
        new(Options.Create(new JwtOptions
        {
            Issuer = "FoodBlog.Tests",
            Audience = "FoodBlog.Tests.Client",
            Secret = "test-secret-with-at-least-32-characters-long",
            AccessTokenMinutes = accessMinutes,
            RefreshTokenDays = 7,
        }));

    [Fact]
    public void CreateAccessToken_ContainsUserClaimsAndRoles()
    {
        var service = CreateService();
        var user = ApplicationUser.Create("Nguyễn Văn A", "a@foodblog.test", "a@foodblog.test");

        var (token, expiresAt) = service.CreateAccessToken(user, ["Author", "Admin"]);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("FoodBlog.Tests", jwt.Issuer);
        Assert.Contains(jwt.Audiences, audience => audience == "FoodBlog.Tests.Client");
        Assert.Contains(jwt.Claims, claim =>
            claim.Type == ClaimTypes.NameIdentifier && claim.Value == user.Id);
        Assert.Contains(jwt.Claims, claim =>
            claim.Type == ClaimTypes.Email && claim.Value == "a@foodblog.test");
        Assert.Contains(jwt.Claims, claim =>
            claim.Type == ClaimTypes.Role && claim.Value == "Author");
        Assert.Contains(jwt.Claims, claim =>
            claim.Type == ClaimTypes.Role && claim.Value == "Admin");
        Assert.True(expiresAt > DateTime.UtcNow);
        Assert.True(expiresAt <= DateTime.UtcNow.AddMinutes(16));
    }

    [Fact]
    public void CreateAccessToken_UsesConfiguredLifetime()
    {
        var service = CreateService(accessMinutes: 5);
        var user = ApplicationUser.Create("Nguyễn Văn B", "b@foodblog.test", "b@foodblog.test");

        var (_, expiresAt) = service.CreateAccessToken(user, ["Author"]);

        Assert.InRange(expiresAt, DateTime.UtcNow.AddMinutes(4), DateTime.UtcNow.AddMinutes(6));
    }

    [Fact]
    public void CreateAccessToken_ThrowsWhenSecretIsTooShort()
    {
        var service = new JwtTokenService(Options.Create(new JwtOptions
        {
            Secret = "short",
        }));
        var user = ApplicationUser.Create("Nguyễn Văn C", "c@foodblog.test", "c@foodblog.test");

        Assert.Throws<InvalidOperationException>(() =>
            service.CreateAccessToken(user, ["Author"]));
    }
}
