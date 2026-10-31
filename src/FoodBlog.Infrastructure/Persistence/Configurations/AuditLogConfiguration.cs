using FoodBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodBlog.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId).HasColumnType("varchar(450)");
        builder.Property(a => a.UserEmail).HasColumnType("varchar(256)");
        builder.Property(a => a.Action).HasColumnType("varchar(100)").IsRequired();
        builder.Property(a => a.EntityType).HasColumnType("varchar(100)").IsRequired();
        builder.Property(a => a.EntityId).HasColumnType("varchar(100)");
        builder.Property(a => a.Description).HasColumnType("varchar(1000)").IsRequired();
        builder.Property(a => a.IpAddress).HasColumnType("varchar(64)");
        builder.Property(a => a.UserAgent).HasColumnType("varchar(512)");
        builder.Property(a => a.CorrelationId).HasColumnType("varchar(100)");
        builder.Property(a => a.Succeeded).HasDefaultValue(true);
        builder.Property(a => a.CreatedAt).HasColumnType("timestamptz").IsRequired();

        // Truy vấn nhật ký theo thời gian / người dùng / hành động là chính.
        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
        builder.HasIndex(a => a.Action);
    }
}
