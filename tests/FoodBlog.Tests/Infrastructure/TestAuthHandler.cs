using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodBlog.Tests.Infrastructure;

/// <summary>
/// Xác thực giả lập cho integration test: luôn đăng nhập với user do
/// <see cref="TestData.SeedRecipeAsync"/> tạo ra, để các endpoint yêu cầu
/// quyền tác giả chạy được mà không cần cookie/Identity thật.
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
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, TestData.AuthorId),
            new Claim(ClaimTypes.Name, "Test Author"),
            new Claim(ClaimTypes.Email, "author@foodblog.test"),
            new Claim(ClaimTypes.Role, "Author"),
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
