using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using FoodBlog.Application.Common.Interfaces;
using FoodBlog.Application.DTOs.Auth;
using FoodBlog.Application.Options;
using FoodBlog.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FoodBlog.Application.Features.Auth.Commands.GoogleLogin;

public sealed class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, AuthResponseDto>
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);
    private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(15);
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IFoodBlogDbContext _db;
    private readonly HttpClient _httpClient;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly GoogleOAuthOptions _googleOptions;

    public GoogleLoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IFoodBlogDbContext db,
        HttpClient httpClient,
        RoleManager<IdentityRole> roleManager,
        IOptions<GoogleOAuthOptions> googleOptions)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
        _httpClient = httpClient;
        _roleManager = roleManager;
        _googleOptions = googleOptions.Value;
    }

    public async Task<AuthResponseDto> Handle(
        GoogleLoginCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code) ||
            string.IsNullOrWhiteSpace(request.CodeVerifier) ||
            string.IsNullOrWhiteSpace(request.RedirectUri))
            throw new UnauthorizedAccessException("Google authorization code không hợp lệ.");

        if (string.IsNullOrWhiteSpace(_googleOptions.ClientId) ||
            string.IsNullOrWhiteSpace(_googleOptions.RedirectUri) ||
            !string.Equals(request.RedirectUri, _googleOptions.RedirectUri, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Google OAuth configuration không hợp lệ.");

        using var tokenRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "https://oauth2.googleapis.com/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = _googleOptions.ClientId,
                ["code"] = request.Code,
                ["code_verifier"] = request.CodeVerifier,
                ["redirect_uri"] = request.RedirectUri,
                ["grant_type"] = "authorization_code"
            })
        };

        if (!string.IsNullOrWhiteSpace(_googleOptions.ClientSecret))
            tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = _googleOptions.ClientId,
                ["client_secret"] = _googleOptions.ClientSecret,
                ["code"] = request.Code,
                ["code_verifier"] = request.CodeVerifier,
                ["redirect_uri"] = request.RedirectUri,
                ["grant_type"] = "authorization_code"
            });

        using var tokenResponse = await _httpClient.SendAsync(
            tokenRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        if (!tokenResponse.IsSuccessStatusCode)
            throw new UnauthorizedAccessException("Không thể đổi Google authorization code.");

        var tokenPayload = await tokenResponse.Content.ReadFromJsonAsync<GoogleTokenResponse>(
            cancellationToken: cancellationToken);
        if (tokenPayload is null || string.IsNullOrWhiteSpace(tokenPayload.AccessToken))
            throw new UnauthorizedAccessException("Google token response không hợp lệ.");

        using var googleRequest = new HttpRequestMessage(
            HttpMethod.Get,
            "https://www.googleapis.com/oauth2/v3/userinfo");
        googleRequest.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenPayload.AccessToken);

        using var googleResponse = await _httpClient.SendAsync(
            googleRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        if (!googleResponse.IsSuccessStatusCode)
            throw new UnauthorizedAccessException("Không thể xác thực tài khoản Google.");

        await using var responseStream = await googleResponse.Content.ReadAsStreamAsync(cancellationToken);
        var googleUser = await JsonSerializer.DeserializeAsync<GoogleUserInfo>(
            responseStream,
            cancellationToken: cancellationToken);
        if (googleUser is null ||
            string.IsNullOrWhiteSpace(googleUser.Sub) ||
            string.IsNullOrWhiteSpace(googleUser.Email) ||
            !googleUser.EmailVerified)
            throw new UnauthorizedAccessException("Tài khoản Google chưa xác thực email.");

        var user = await _userManager.FindByLoginAsync("Google", googleUser.Sub);
        if (user is null)
        {
            user = await _userManager.FindByEmailAsync(googleUser.Email);
            if (user is null)
            {
                user = ApplicationUser.Create(
                    string.IsNullOrWhiteSpace(googleUser.Name)
                        ? googleUser.Email.Split('@')[0]
                        : googleUser.Name,
                    googleUser.Email,
                    googleUser.Email);
                user.EmailConfirmed = true;

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    throw new InvalidOperationException(
                        string.Join(", ", createResult.Errors.Select(error => error.Description)));

                if (!await _roleManager.RoleExistsAsync("Author"))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole("Author"));
                    if (!roleResult.Succeeded && !await _roleManager.RoleExistsAsync("Author"))
                    {
                        await _userManager.DeleteAsync(user);
                        throw new InvalidOperationException("Không thể khởi tạo role Author.");
                    }
                }

                var authorResult = await _userManager.AddToRoleAsync(user, "Author");
                if (!authorResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    throw new InvalidOperationException(
                        string.Join(", ", authorResult.Errors.Select(error => error.Description)));
                }
            }

            var existingLogin = await _userManager.FindByLoginAsync("Google", googleUser.Sub);
            if (existingLogin is null)
            {
                var loginResult = await _userManager.AddLoginAsync(
                    user,
                    new UserLoginInfo("Google", googleUser.Sub, "Google"));
                if (!loginResult.Succeeded)
                    throw new InvalidOperationException(
                        string.Join(", ", loginResult.Errors.Select(error => error.Description)));
            }
        }

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Tài khoản đã bị vô hiệu hóa.");

        await _signInManager.SignInAsync(user, isPersistent: false);

        if (!string.IsNullOrWhiteSpace(googleUser.Picture) &&
            string.IsNullOrWhiteSpace(user.AvatarUrl))
        {
            user.AvatarUrl = googleUser.Picture;
            await _userManager.UpdateAsync(user);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var rawRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        _db.RefreshTokens.Add(global::FoodBlog.Domain.Entities.RefreshToken.Create(
            user.Id,
            HashToken(rawRefreshToken),
            DateTime.UtcNow.Add(RefreshTokenLifetime),
            request.IpAddress));
        await _db.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            AccessToken: string.Empty,
            RefreshToken: rawRefreshToken,
            ExpiresAt: DateTime.UtcNow.Add(AccessTokenLifetime),
            User: new UserDto(
                user.Id,
                user.Email ?? googleUser.Email,
                user.DisplayName,
                roles.FirstOrDefault() ?? "Author"));
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)))
            .ToLowerInvariant();

    private sealed record GoogleTokenResponse(
        [property: System.Text.Json.Serialization.JsonPropertyName("access_token")]
        string? AccessToken,
        [property: System.Text.Json.Serialization.JsonPropertyName("id_token")]
        string? IdToken);

    private sealed record GoogleUserInfo(
        string? Sub,
        string? Email,
        string? Name,
        string? Picture,
        [property: System.Text.Json.Serialization.JsonPropertyName("email_verified")]
        bool EmailVerified);
}
