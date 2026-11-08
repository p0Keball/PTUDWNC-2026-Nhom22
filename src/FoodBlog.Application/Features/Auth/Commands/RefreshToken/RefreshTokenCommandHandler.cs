using System.Security.Cryptography;
using System.Text;
using FoodBlog.Application.Common.Interfaces;
using FoodBlog.Application.DTOs.Auth;
using FoodBlog.Application.Options;
using FoodBlog.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodBlog.Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IFoodBlogDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IFoodBlogDbContext db,
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        IOptions<JwtOptions> jwtOptions,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _db = db;
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new UnauthorizedAccessException("Refresh token không hợp lệ.");

        var tokenHash = HashToken(request.RefreshToken);
        var storedToken = await _db.RefreshTokens
            .Include(token => token.User)
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (storedToken?.User is null || !storedToken.User.IsActive)
            throw new UnauthorizedAccessException("Refresh token không hợp lệ hoặc đã hết hạn.");

        if (!storedToken.IsActive)
        {
            if (!string.IsNullOrWhiteSpace(storedToken.ReplacedByTokenHash))
            {
                _logger.LogWarning(
                    "Refresh token reuse detected for user {UserId} from IP {IpAddress}. Revoking token family.",
                    storedToken.User.Id,
                    request.IpAddress ?? "unknown");

                var activeTokens = await _db.RefreshTokens
                    .Where(token => token.UserId == storedToken.UserId && token.RevokedAt == null)
                    .ToListAsync(cancellationToken);

                var revokedAt = DateTime.UtcNow;
                foreach (var token in activeTokens)
                    token.RevokedAt = revokedAt;

                await _db.SaveChangesAsync(cancellationToken);
            }

            throw new UnauthorizedAccessException("Refresh token không hợp lệ hoặc đã được sử dụng.");
        }

        var replacementToken = CreateRawToken();
        var replacementHash = HashToken(replacementToken);
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.ReplacedByTokenHash = replacementHash;

        _db.RefreshTokens.Add(global::FoodBlog.Domain.Entities.RefreshToken.Create(
            storedToken.User.Id,
            replacementHash,
            DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
            request.IpAddress));

        await _db.SaveChangesAsync(cancellationToken);

        var roles = await _userManager.GetRolesAsync(storedToken.User);
        var (accessToken, expiresAt) = _jwtTokenService.CreateAccessToken(storedToken.User, roles);
        return new AuthResponseDto(
            AccessToken: accessToken,
            RefreshToken: replacementToken,
            ExpiresAt: expiresAt,
            User: new UserDto(
                storedToken.User.Id,
                storedToken.User.Email ?? string.Empty,
                storedToken.User.DisplayName,
                roles.FirstOrDefault() ?? "Author"));
    }

    private static string CreateRawToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)))
            .ToLowerInvariant();
}