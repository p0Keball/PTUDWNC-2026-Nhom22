using System.Security.Claims;
using System.Security.Cryptography;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Hangfire;
using FoodBlog.Application.DTOs.Auth;
using FoodBlog.Infrastructure.Jobs;
using FoodBlog.Application.Features.Auth.Commands.GoogleLogin;
using FoodBlog.Application.Features.Auth.Commands.Logout;
using FoodBlog.Application.Features.Auth.Commands.RefreshToken;
using FoodBlog.Application.Features.Auth.Commands.UpdateMe;
using FoodBlog.Application.Features.Auth.Queries.GetMe;
using FoodBlog.Application.Interfaces;
using FoodBlog.Domain.Entities;
using FoodBlog.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace FoodBlog.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(
        this IEndpointRouteBuilder app,
        ApiVersionSet apiVersionSet)
    {
        var group = app.MapGroup("/api/v{version:apiVersion}/auth")
            .WithApiVersionSet(apiVersionSet)
            .HasApiVersion(new ApiVersion(1, 0))
            .WithTags("Authentication")
            .RequireRateLimiting("auth");

        group.MapPost("/register", Register);
        group.MapPost("/login", Login);
        group.MapPost("/google", GoogleLogin);
        group.MapPost("/refresh", Refresh);
        group.MapPost("/logout", Logout).RequireAuthorization();
        group.MapPost("/change-password", ChangePassword).RequireAuthorization();
        group.MapPost("/forgot-password", ForgotPassword);
        group.MapPost("/reset-password", ResetPassword);

        group.MapGet("/me", GetMe)
            .RequireAuthorization()
            .WithName("GetCurrentUserProfile")
            .WithSummary("Xem hồ sơ người dùng hiện tại")
            .WithDescription("Trả về thông tin hồ sơ của tài khoản đang đăng nhập.")
            .Produces<UserProfileDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPatch("/me", UpdateMe)
            .RequireAuthorization()
            .WithName("UpdateCurrentUserProfile")
            .WithSummary("Cập nhật hồ sơ người dùng hiện tại")
            .WithDescription("Cập nhật từng phần tên hiển thị, avatar hoặc tiểu sử của tài khoản đang đăng nhập.")
            .Produces<UserProfileDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status422UnprocessableEntity);

        return app;
    }

    private static async Task<IResult> Register(
        RegisterRequest request,
        UserManager<ApplicationUser> users,
        RoleManager<IdentityRole> roles,
        SignInManager<ApplicationUser> signInManager,
        FoodBlogDbContext db,
        IBackgroundJobClient backgroundJobs,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var email = request.Email.Trim();
        var displayName = request.DisplayName.Trim();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(displayName))
            return Results.BadRequest(new { title = "Email, mật khẩu và tên hiển thị là bắt buộc." });

        var user = ApplicationUser.Create(displayName, email, email);
        var result = await users.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return Results.UnprocessableEntity(new
            {
                title = "Đăng ký không thành công.",
                errors = result.Errors.GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray())
            });

        if (!await roles.RoleExistsAsync("Author"))
        {
            var roleResult = await roles.CreateAsync(new IdentityRole("Author"));
            if (!roleResult.Succeeded && !await roles.RoleExistsAsync("Author"))
            {
                await users.DeleteAsync(user);
                return Results.Problem(
                    title: "Không thể khởi tạo role Author.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        var roleAssignment = await users.AddToRoleAsync(user, "Author");
        if (!roleAssignment.Succeeded)
        {
            await users.DeleteAsync(user);
            return Results.Problem(
                title: "Không thể gán role Author.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var refreshToken = await CreateRefreshTokenAsync(user, db, httpContext, ct);
        await signInManager.SignInAsync(user, isPersistent: false);
        backgroundJobs.Enqueue<WelcomeEmailJob>(job =>
            job.SendAsync(user.Email!, user.DisplayName, CancellationToken.None));
        return Results.Ok(ToResponse(user, refreshToken));
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        UserManager<ApplicationUser> users,
        SignInManager<ApplicationUser> signInManager,
        FoodBlogDbContext db,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var email = request.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
            return Results.BadRequest(new
            {
                title = "Email và mật khẩu là bắt buộc."
            });

        var user = await users.FindByEmailAsync(email);
        if (user is null || !user.IsActive)
            return Results.Unauthorized();

        if (!user.LockoutEnabled)
        {
            user.LockoutEnabled = true;
            var lockoutUpdate = await users.UpdateAsync(user);
            if (!lockoutUpdate.Succeeded)
                return Results.Problem(
                    title: "Không thể khởi tạo cơ chế khóa tài khoản.",
                    statusCode: StatusCodes.Status500InternalServerError);
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            var retryAfterSeconds = user.LockoutEnd.HasValue
                ? Math.Max(0, (int)Math.Ceiling((user.LockoutEnd.Value - DateTimeOffset.UtcNow).TotalSeconds))
                : 900;

            httpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString(
                System.Globalization.CultureInfo.InvariantCulture);
            return Results.Problem(
                title: "Tài khoản đã bị khóa.",
                detail: "Tài khoản bị khóa do đăng nhập sai quá 5 lần. Vui lòng thử lại sau.",
                statusCode: StatusCodes.Status423Locked);
        }

        if (!result.Succeeded)
            return Results.Unauthorized();

        var refreshToken = await CreateRefreshTokenAsync(user, db, httpContext, ct);
        await signInManager.SignInAsync(user, isPersistent: false);
        return Results.Ok(ToResponse(user, refreshToken));
    }

    private static async Task<IResult> Refresh(
        RefreshRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken ct)
    {
        try
        {
            var result = await sender.Send(
                new RefreshTokenCommand(
                    request.RefreshToken,
                    httpContext.Connection.RemoteIpAddress?.ToString()),
                ct);

            return Results.Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
    }

    private static async Task<IResult> GoogleLogin(
        GoogleLoginRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken ct)
    {
        try
        {
            var result = await sender.Send(
                new GoogleLoginCommand(
                    request.Code,
                    request.CodeVerifier,
                    request.RedirectUri,
                    httpContext.Connection.RemoteIpAddress?.ToString()),
                ct);
            return Results.Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (InvalidOperationException exception)
        {
            return Results.UnprocessableEntity(new
            {
                title = "Đăng nhập Google không thành công.",
                detail = exception.Message
            });
        }
    }

    private static async Task<IResult> Logout(
        ClaimsPrincipal principal,
        ISender sender,
        SignInManager<ApplicationUser> signInManager,
        CancellationToken ct)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        await sender.Send(new LogoutCommand(userId), ct);
        await signInManager.SignOutAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> GetMe(
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        try
        {
            var profile = await sender.Send(new GetMeQuery(userId), ct);
            return Results.Ok(profile);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
    }

    private static async Task<IResult> UpdateMe(
        UpdateProfileRequest request,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken ct)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        try
        {
            var profile = await sender.Send(new UpdateMeCommand(userId, request), ct);
            return Results.Ok(profile);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (ArgumentException exception)
        {
            return Results.UnprocessableEntity(new
            {
                title = "Dữ liệu hồ sơ không hợp lệ.",
                detail = exception.Message
            });
        }
        catch (InvalidOperationException exception)
        {
            return Results.UnprocessableEntity(new
            {
                title = "Cập nhật hồ sơ không thành công.",
                detail = exception.Message
            });
        }
    }

    private static async Task<IResult> ChangePassword(
        ChangePasswordRequest request,
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> users,
        SignInManager<ApplicationUser> signInManager)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
        var user = string.IsNullOrWhiteSpace(userId) ? null : await users.FindByIdAsync(userId);
        if (user is null) return Results.Unauthorized();
        var result = await users.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return Results.UnprocessableEntity(new
            {
                title = "Đổi mật khẩu không thành công.",
                errors = result.Errors.GroupBy(e => e.Code).ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray())
            });
        await signInManager.RefreshSignInAsync(user);
        return Results.NoContent();
    }

    private static async Task<IResult> ForgotPassword(
        ForgotPasswordRequest request,
        UserManager<ApplicationUser> users,
        IEmailSender emailSender,
        IConfiguration configuration,
        CancellationToken ct)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim());
        if (user is not null && user.IsActive && !string.IsNullOrWhiteSpace(user.Email))
        {
            var token = await users.GeneratePasswordResetTokenAsync(user);
            var baseUrl = configuration["Frontend:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:3000";
            var resetUrl = $"{baseUrl}/auth/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";
            await emailSender.SendPasswordResetEmailAsync(user.Email, user.DisplayName, resetUrl, ct);
        }
        return Results.Ok(new { message = "Nếu email tồn tại, hướng dẫn đặt lại mật khẩu đã được gửi." });
    }

    private static async Task<IResult> ResetPassword(
        ResetPasswordRequest request,
        UserManager<ApplicationUser> users)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim());
        if (user is null) return Results.BadRequest(new { title = "Liên kết đặt lại mật khẩu không hợp lệ." });
        var result = await users.ResetPasswordAsync(user, request.Token, request.NewPassword);
        return result.Succeeded
            ? Results.NoContent()
            : Results.UnprocessableEntity(new
            {
                title = "Không thể đặt lại mật khẩu.",
                errors = result.Errors.GroupBy(e => e.Code).ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray())
            });
    }

    private static async Task<string> CreateRefreshTokenAsync(
        ApplicationUser user, FoodBlogDbContext db, HttpContext httpContext, CancellationToken ct)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        db.RefreshTokens.Add(RefreshToken.Create(
            user.Id, HashToken(rawToken), DateTime.UtcNow.AddDays(30),
            httpContext.Connection.RemoteIpAddress?.ToString()));
        await db.SaveChangesAsync(ct);
        return rawToken;
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

    private static AuthResponse ToResponse(ApplicationUser user, string refreshToken) =>
        new(ToProfile(user), refreshToken);

    private static ProfileResponse ToProfile(ApplicationUser user) =>
        new(user.Id, user.Email ?? string.Empty, user.DisplayName, user.AvatarUrl, user.Bio);

    private sealed record RegisterRequest(string DisplayName, string Email, string Password);
    private sealed record LoginRequest(string Email, string Password);
    private sealed record RefreshRequest(string RefreshToken);
    private sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
    private sealed record ForgotPasswordRequest(string Email);
    private sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);
    private sealed record ProfileResponse(string Id, string Email, string DisplayName, string? AvatarUrl, string? Bio);
    private sealed record AuthResponse(ProfileResponse User, string RefreshToken);
}