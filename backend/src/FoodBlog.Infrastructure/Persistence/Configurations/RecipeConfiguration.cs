using FoodBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodBlog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API cho Recipe (SRS §7.2). Skeleton Chương 1.
/// </summary>
public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.ToTable("Recipes");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Slug)
            .IsRequired()
            .HasMaxLength(220);

        builder.HasIndex(r => r.Slug).IsUnique();

        builder.Property(r => r.Description)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(r => r.Status).IsRequired();
        builder.Property(r => r.Difficulty).IsRequired();

        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.CategoryId);

        // 1 Recipe -> N Step, Cascade Delete (SRS §6.4)
        builder.HasMany(r => r.Steps)
            .WithOne(s => s.Recipe!)
            .HasForeignKey(s => s.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
