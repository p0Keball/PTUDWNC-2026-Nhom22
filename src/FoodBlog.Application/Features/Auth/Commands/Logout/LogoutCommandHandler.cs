using System.Security.Cryptography;
using System.Text;
using FoodBlog.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.Application.Features.Auth.Commands.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IFoodBlogDbContext _db;

    public LogoutCommandHandler(IFoodBlogDbContext db)
    {
        _db = db;
    }

    public async Task Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            throw new UnauthorizedAccessException("Không xác định được người dùng.");

        var query = _db.RefreshTokens
            .Where(token => token.UserId == request.UserId && token.RevokedAt == null);

        // FR-AUTH-005: revoke đúng refresh token client gửi lên.
        // Nếu client cũ không gửi body, fallback revoke toàn bộ token của user (idempotent).
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            var tokenHash = HashToken(request.RefreshToken);
            query = query.Where(token => token.TokenHash == tokenHash);
        }

        var activeTokens = await query.ToListAsync(cancellationToken);
        var revokedAt = DateTime.UtcNow;
        foreach (var token in activeTokens)
            token.RevokedAt = revokedAt;

        if (activeTokens.Count > 0)
            await _db.SaveChangesAsync(cancellationToken);
    }

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)))
            .ToLowerInvariant();
}
