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

        var activeTokens = await _db.RefreshTokens
            .Where(token => token.UserId == request.UserId && token.RevokedAt == null)
            .ToListAsync(cancellationToken);

        var revokedAt = DateTime.UtcNow;
        foreach (var token in activeTokens)
            token.RevokedAt = revokedAt;

        if (activeTokens.Count > 0)
            await _db.SaveChangesAsync(cancellationToken);
    }
}
