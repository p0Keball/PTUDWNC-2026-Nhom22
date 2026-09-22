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

        // Owned Entity: nhúng vào bảng Recipes với tiền tố Nutrition_* (SRS §7.2.1)
        builder.OwnsOne(r => r.Nutrition, nb =>
        {
            nb.Property(n => n.Calories).HasColumnName("Nutrition_Calories").HasPrecision(8, 2);
            nb.Property(n => n.Protein).HasColumnName("Nutrition_Protein").HasPrecision(8, 2);
            nb.Property(n => n.Carbohydrates).HasColumnName("Nutrition_Carbohydrates").HasPrecision(8, 2);
            nb.Property(n => n.Fat).HasColumnName("Nutrition_Fat").HasPrecision(8, 2);
            nb.Property(n => n.Fiber).HasColumnName("Nutrition_Fiber").HasPrecision(8, 2);
            nb.Property(n => n.Sodium).HasColumnName("Nutrition_Sodium").HasPrecision(8, 2);
        });

        // N Recipe -> 1 Author, Restrict (xóa user không cascade xóa recipe)
        builder.HasOne(r => r.Author)
            .WithMany(u => u.Recipes)
            .HasForeignKey(r => r.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // 1 Recipe -> N Step, Cascade Delete (SRS §6.4)
        builder.HasMany(r => r.Steps)
            .WithOne(s => s.Recipe!)
            .HasForeignKey(s => s.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}
