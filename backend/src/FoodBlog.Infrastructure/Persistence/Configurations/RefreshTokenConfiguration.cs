using FoodBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodBlog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API cho RefreshToken (SRS §7.8).
/// TokenHash SHA-256 hex unique. Không có IsRevoked — logic sống/chết
/// nằm ở domain (RevokedAt == null && chưa hết hạn).
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TokenHash)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(t => t.TokenHash).IsUnique();

        builder.Property(t => t.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(t => t.ReplacedByTokenHash)
            .HasMaxLength(64);

        builder.Property(t => t.CreatedByIp)
            .HasMaxLength(45);

        builder.HasIndex(t => t.ExpiresAt);

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
