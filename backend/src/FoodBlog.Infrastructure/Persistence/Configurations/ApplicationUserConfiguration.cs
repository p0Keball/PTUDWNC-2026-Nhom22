using FoodBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodBlog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API cho ApplicationUser (SRS §7.7).
/// QUYẾT ĐỊNH: map sẵn vào bảng "AspNetUsers" để khi Chương 2引入
/// Identity (ApplicationUser : IdentityUser) thì tên bảng giữ nguyên,
/// migration sau chỉ thêm cột Identity, không phải đổi tên bảng.
/// </summary>
public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("AspNetUsers");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasMaxLength(450);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.UserName).IsUnique();

        builder.Property(u => u.DisplayName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(u => u.Bio)
            .HasColumnType("text");

        // 1 User -> N RefreshToken, Cascade (xóa user => thu hồi hết phiên)
        builder.HasMany(u => u.RefreshTokens)
            .WithOne(t => t.User!)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
