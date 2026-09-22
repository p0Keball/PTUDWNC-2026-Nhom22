using FoodBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodBlog.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        BaseEntityConfig.Configure(builder);
        builder.ToTable("RefreshTokens");
        builder.Property(t => t.UserId)
            .HasColumnType("varchar(450)")
            .IsRequired();
        builder.Property(t => t.TokenHash)
            .HasColumnType("varchar(64)")
            .IsRequired();
        builder.HasIndex(t => t.TokenHash).IsUnique();
        builder.Property(t => t.ExpiresAt).HasColumnType("timestamptz");
        builder.Property(t => t.RevokedAt).HasColumnType("timestamptz");
        builder.Property(t => t.ReplacedByTokenHash).HasColumnType("varchar(64)");
        builder.Property(t => t.CreatedByIp).HasColumnType("varchar(45)");
        builder.HasOne(t => t.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
