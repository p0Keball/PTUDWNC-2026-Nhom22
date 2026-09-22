using FoodBlog.Domain.Common;

namespace FoodBlog.Domain.Entities;

/// <summary>
/// Phiên đăng nhập (SRS §7.8: RefreshTokens).
/// QUYẾT ĐỊNH (FAILURE F-01/F-02):
/// - Chỉ lưu SHA-256 hash của token (TokenHash, unique), KHÔNG lưu plain text.
///   Plain text chỉ trả cho client đúng 1 lần lúc cấp.
/// - KHÔNG có cột IsRevoked: còn sống <=> RevokedAt == null.
/// - Rotation: token cũ set RevokedAt + ReplacedByTokenHash, cấp token mới.
///   Phát hiện reuse (token đã revoke bị dùng lại) => revoke toàn bộ (paranoid).
/// </summary>
public class RefreshToken : BaseEntity
{
    public string UserId { get; private set; } = string.Empty;
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }
    public string? CreatedByIp { get; private set; }

    // Navigation
    public ApplicationUser? User { get; private set; }

    public bool IsActive => RevokedAt is null && ExpiresAt > DateTime.UtcNow;

    protected RefreshToken() { }

    public static RefreshToken Create(string userId, string tokenHash, DateTime expiresAt, string? createdByIp = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId, nameof(userId));
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash, nameof(tokenHash));

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedByIp = createdByIp,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Revoke(string? replacedByTokenHash = null)
    {
        RevokedAt = DateTime.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
        UpdatedAt = DateTime.UtcNow;
    }
}
