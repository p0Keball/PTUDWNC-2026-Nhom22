using FoodBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodBlog.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API cho RecipeImage (SRS §7.5). Cascade khi xóa Recipe.
/// </summary>
public class RecipeImageConfiguration : IEntityTypeConfiguration<RecipeImage>
{
    public void Configure(EntityTypeBuilder<RecipeImage> builder)
    {
        builder.ToTable("RecipeImages");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.OriginalUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.MediumUrl)
            .HasMaxLength(500);

        builder.Property(i => i.ThumbnailUrl)
            .HasMaxLength(500);

        builder.Property(i => i.AltText)
            .HasMaxLength(200);

        builder.HasOne(i => i.Recipe)
            .WithMany(r => r.Images)
            .HasForeignKey(i => i.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(i => !i.IsDeleted);
    }
}
