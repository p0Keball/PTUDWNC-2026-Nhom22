using FoodBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodBlog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API cho Category (SRS §7.6).
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(c => c.Name).IsUnique();

        builder.Property(c => c.Slug)
            .IsRequired()
            .HasMaxLength(120);

        builder.HasIndex(c => c.Slug).IsUnique();

        builder.Property(c => c.Description)
            .HasColumnType("text");

        builder.Property(c => c.ImageUrl)
            .HasMaxLength(500);

        // 1 Category -> N Recipe, Restrict Delete (FR-CAT-005)
        builder.HasMany(c => c.Recipes)
            .WithOne(r => r.Category!)
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Global Query Filter: ẩn soft-deleted
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
