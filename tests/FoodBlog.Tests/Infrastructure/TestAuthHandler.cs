using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodBlog.Tests.Infrastructure;

/// <summary>
/// Xác thực giả lập cho integration test:
/// - Mặc định: đăng nhập với role Author (TestData.AuthorId).
/// - Header X-Test-Anonymous: true -> Chưa đăng nhập (Guest).
/// - Header X-Test-Role: Admin -> Đăng nhập với role Admin.
/// - Header X-Test-UserId: {id} -> Đăng nhập với userId tùy biến.
/// </summary>
internal sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers.ContainsKey("X-Test-Anonymous"))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userId = Request.Headers.TryGetValue("X-Test-UserId", out var uid)
            ? uid.ToString()
            : TestData.AuthorId;

        var role = Request.Headers.TryGetValue("X-Test-Role", out var r)
            ? r.ToString()
            : "Author";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, $"Test User {userId}"),
            new(ClaimTypes.Email, $"{userId}@foodblog.test"),
            new(ClaimTypes.Role, role),
        };

        if (role == "Admin")
        {
            claims.Add(new Claim(ClaimTypes.Role, "Author"));
        }

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
