using FoodBlog.Domain.Common;

namespace FoodBlog.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public string? CreatedByIp { get; set; }

    public ApplicationUser? User { get; private set; } = default!;

     public bool IsRevoked => RevokedAt is not null;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    public static RefreshToken Create(string userId, string tokenHash, DateTime expiresAt, string? ip) => new()
    {
        UserId = userId, TokenHash = tokenHash, ExpiresAt = expiresAt, CreatedByIp = ip
    };
}
