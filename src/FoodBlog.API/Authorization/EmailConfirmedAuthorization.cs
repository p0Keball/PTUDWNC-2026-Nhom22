using Microsoft.AspNetCore.Authorization;

namespace FoodBlog.API.Authorization;

/// <summary>
/// SRS 2.3 - tầng 3: Policy "VerifiedAuthor" yêu cầu email đã xác nhận.
/// </summary>
public sealed class EmailConfirmedRequirement : IAuthorizationRequirement;

/// <summary>
/// Kiểm tra claim <c>email_confirmed</c> do JwtTokenService phát hành.
/// Cờ <c>Auth:RequireConfirmedEmail</c> cho phép tắt kiểm tra ở dev/demo
/// (mặc định tắt để không khóa các tài khoản seed chưa xác nhận email).
/// </summary>
public sealed class EmailConfirmedAuthorizationHandler(IConfiguration configuration)
    : AuthorizationHandler<EmailConfirmedRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EmailConfirmedRequirement requirement)
    {
        var required = bool.TryParse(
            configuration["Auth:RequireConfirmedEmail"],
            out var enabled) && enabled;

        if (!required ||
            context.User.IsInRole("Admin") ||
            context.User.HasClaim("email_confirmed", "true"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
