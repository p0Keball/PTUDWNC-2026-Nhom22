using System.Security.Claims;
using System.Security.Cryptography;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Hangfire;
using FoodBlog.Application.Common.Interfaces;
using FoodBlog.Application.DTOs.Auth;
using FoodBlog.Application.Options;
using FoodBlog.Domain.Entities;
using FoodBlog.Domain.Exceptions;
using FoodBlog.Infrastructure.Jobs;
using FoodBlog.Application.Features.Auth.Commands.ChangePassword;
using FoodBlog.Application.Features.Auth.Commands.ForgotPassword;
using FoodBlog.Application.Features.Auth.Commands.GoogleLogin;
using FoodBlog.Application.Features.Auth.Commands.Logout;
using FoodBlog.Application.Features.Auth.Commands.RefreshToken;
using FoodBlog.Application.Features.Auth.Commands.ResetPassword;
using FoodBlog.Application.Features.Auth.Commands.UpdateMe;
using FoodBlog.Application.Features.Auth.Queries.GetMe;
using FoodBlog.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
        group.MapPost("/change-password", ChangePassword).RequireAuthorization();
        group.MapPost("/forgot-password", ForgotPassword);
        group.MapPost("/reset-password", ResetPassword);
        group.MapPost("/logout", Logout).RequireAuthorization();

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
        IAuditLogger audit,
        IJwtTokenService jwt,
        IOptions<JwtOptions> jwtOptions,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var email = request.Email?.Trim();
        var password = request.Password;
        var fullName = (request.FullName ?? request.DisplayName ?? string.Empty).Trim();
        var userName = string.IsNullOrWhiteSpace(request.UserName)
            ? email
            : request.UserName.Trim();

        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(email))
            errors["email"] = ["Email là bắt buộc."];
        else if (!email.Contains('@') || email.Length > 256)
            errors["email"] = ["Email không đúng định dạng."];
        if (string.IsNullOrWhiteSpace(password))
            errors["password"] = ["Mật khẩu là bắt buộc."];
        if (fullName.Length is < 2 or > 100)
            errors["fullName"] = ["Họ tên phải từ 2 đến 100 ký tự."];
        if (string.IsNullOrWhiteSpace(userName))
            errors["userName"] = ["Tên đăng nhập là bắt buộc."];
        if (errors.Count > 0)
            return Results.UnprocessableEntity(new { title = "Dữ liệu đăng ký không hợp lệ.", errors });

        if (await users.FindByEmailAsync(email!) is not null)
            throw new ConflictException("Email đã được sử dụng.", "AUTH_EMAIL_EXISTS");
        if (await users.FindByNameAsync(userName!) is not null)
            throw new ConflictException("Tên đăng nhập đã được sử dụng.", "AUTH_USERNAME_EXISTS");

        var user = ApplicationUser.Create(fullName, email!, userName!);
        var result = await users.CreateAsync(user, password!);
        if (!result.Succeeded)
        {
            if (result.Errors.Any(error =>
                    error.Code is "DuplicateEmail" or "DuplicateUserName"))
                throw new ConflictException("Email hoặc tên đăng nhập đã được sử dụng.", "AUTH_EMAIL_EXISTS");

            return Results.UnprocessableEntity(new
            {
                title = "Đăng ký không thành công.",
                errors = result.Errors.GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray())
            });
        }

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

        var refreshToken = await CreateRefreshTokenAsync(
            user, db, httpContext, jwtOptions.Value.RefreshTokenDays, ct);
        await signInManager.SignInAsync(user, isPersistent: false);
        backgroundJobs.Enqueue<WelcomeEmailJob>(job =>
            job.SendAsync(user.Email!, user.DisplayName, CancellationToken.None));
        await audit.LogAsync(new AuditLogEntry("USER_REGISTERED", "User", user.Id,
            $"Đăng ký tài khoản {user.Email}", UserId: user.Id, UserEmail: user.Email), ct);

        var response = await BuildAuthResponseAsync(user, refreshToken, users, jwt);
        return Results.Created($"/api/v1/auth/me", response);
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        UserManager<ApplicationUser> users,
        SignInManager<ApplicationUser> signInManager,
        FoodBlogDbContext db,
        IAuditLogger audit,
        IJwtTokenService jwt,
        IOptions<JwtOptions> jwtOptions,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var email = request.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
            return Results.BadRequest(new { title = "Email và mật khẩu là bắt buộc." });

        var user = await users.FindByEmailAsync(email);
        if (user is null || !user.IsActive)
        {
            await audit.LogAsync(new AuditLogEntry("USER_LOGIN_FAILED", "Auth", null,
                $"Đăng nhập thất bại cho {email}", Succeeded: false, UserEmail: email), ct);
            return Results.Unauthorized();
        }

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
            await audit.LogAsync(new AuditLogEntry("USER_LOGIN_LOCKED", "Auth", user.Id,
                $"Tài khoản {user.Email} bị khóa do sai mật khẩu quá 5 lần",
                Succeeded: false, UserId: user.Id, UserEmail: user.Email), ct);
            return Results.Problem(
                title: "Tài khoản đã bị khóa.",
                detail: "Tài khoản bị khóa do đăng nhập sai quá 5 lần. Vui lòng thử lại sau.",
                statusCode: StatusCodes.Status423Locked);
        }

        if (!result.Succeeded)
        {
            await audit.LogAsync(new AuditLogEntry("USER_LOGIN_FAILED", "Auth", user.Id,
                $"Sai mật khẩu cho {user.Email}", Succeeded: false, UserId: user.Id, UserEmail: user.Email), ct);
            return Results.Unauthorized();
        }

        var refreshToken = await CreateRefreshTokenAsync(
            user, db, httpContext, jwtOptions.Value.RefreshTokenDays, ct);
        await signInManager.SignInAsync(user, isPersistent: false);
        await audit.LogAsync(new AuditLogEntry("USER_LOGGED_IN", "Auth", user.Id,
            $"Đăng nhập thành công {user.Email}", UserId: user.Id, UserEmail: user.Email), ct);

        return Results.Ok(await BuildAuthResponseAsync(user, refreshToken, users, jwt));
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
                    request.RefreshToken ?? string.Empty,
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
        LogoutRequest? request,
        ClaimsPrincipal principal,
        ISender sender,
        IAuditLogger audit,
        SignInManager<ApplicationUser> signInManager,
        CancellationToken ct)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        await sender.Send(new LogoutCommand(userId, request?.RefreshToken), ct);
        await signInManager.SignOutAsync();
        await audit.LogAsync(new AuditLogEntry("USER_LOGGED_OUT", "Auth", userId,
            "Đăng xuất"), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ChangePassword(
        ChangePasswordRequest request,
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> users,
        IAuditLogger audit,
        CancellationToken ct)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        var user = await users.FindByIdAsync(userId);
        if (user is null || !user.IsActive)
            return Results.NotFound(new { title = "Tài khoản không tồn tại." });

        var isCurrentPasswordCorrect = await users.CheckPasswordAsync(user, request.CurrentPassword);
        if (!isCurrentPasswordCorrect)
            return Results.UnprocessableEntity(new
            {
                title = "Mật khẩu hiện tại không chính xác.",
                errors = new { currentPassword = new[] { "Mật khẩu hiện tại không đúng." } }
            });

        var result = await users.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return Results.UnprocessableEntity(new
            {
                title = "Đổi mật khẩu thất bại.",
                errors = result.Errors.GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray())
            });

        await audit.LogAsync(new AuditLogEntry("USER_PASSWORD_CHANGED", "Auth", userId,
            "Đổi mật khẩu thành công"), ct);

        return Results.Ok(new { message = "Đã đổi mật khẩu thành công." });
    }

    private static async Task<IResult> ForgotPassword(
        ForgotPasswordRequest request,
        ISender sender,
        CancellationToken ct)
    {
        await sender.Send(new ForgotPasswordCommand(request), ct);
        // Luôn trả về thành công để không tiết lộ email có tồn tại hay không.
        return Results.Ok(new { message = "Nếu email tồn tại, liên kết khôi phục đã được gửi." });
    }

    private static async Task<IResult> ResetPassword(
        ResetPasswordRequest request,
        ISender sender,
        CancellationToken ct)
    {
        await sender.Send(new ResetPasswordCommand(request), ct);
        return Results.Ok(new { message = "Đã đặt lại mật khẩu thành công." });
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

    private static async Task<string> CreateRefreshTokenAsync(
        ApplicationUser user,
        FoodBlogDbContext db,
        HttpContext httpContext,
        int refreshTokenDays,
        CancellationToken ct)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        db.RefreshTokens.Add(RefreshToken.Create(
            user.Id, HashToken(rawToken), DateTime.UtcNow.AddDays(refreshTokenDays),
            httpContext.Connection.RemoteIpAddress?.ToString()));
        await db.SaveChangesAsync(ct);
        return rawToken;
    }

    private static async Task<AuthResponseDto> BuildAuthResponseAsync(
        ApplicationUser user,
        string refreshToken,
        UserManager<ApplicationUser> users,
        IJwtTokenService jwt)
    {
        var roles = await users.GetRolesAsync(user);
        var (accessToken, expiresAt) = jwt.CreateAccessToken(user, roles);
        return new AuthResponseDto(
            accessToken,
            refreshToken,
            expiresAt,
            new UserDto(
                user.Id,
                user.Email ?? string.Empty,
                user.DisplayName,
                roles.FirstOrDefault() ?? "Author",
                user.UserName,
                user.AvatarUrl,
                roles.ToArray()));
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

    private sealed record RegisterRequest(
        string? FullName,
        string? UserName,
        string? DisplayName,
        string? Email,
        string? Password);

    private sealed record LoginRequest(string? Email, string? Password);
    private sealed record RefreshRequest(string? RefreshToken);
    private sealed record LogoutRequest(string? RefreshToken);
}
