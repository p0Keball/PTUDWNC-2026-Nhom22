using FoodBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodBlog.Infrastructure.Persistence.Configurations;

public class RecipeImageConfiguration : IEntityTypeConfiguration<RecipeImage>
{
    public void Configure(EntityTypeBuilder<RecipeImage> builder)
    {
        BaseEntityConfig.Configure(builder);
        builder.ToTable("RecipeImages");
        builder.Property(i => i.OriginalUrl)
            .HasColumnType("varchar(500)")
            .IsRequired();
        builder.Property(i => i.MediumUrl).HasColumnType("varchar(500)");
        builder.Property(i => i.ThumbnailUrl).HasColumnType("varchar(500)");
        builder.Property(i => i.AltText).HasColumnType("varchar(200)");
        builder.Property(i => i.IsPrimary).HasDefaultValue(false);
        builder.Property(i => i.OrderIndex).HasDefaultValue(0);
    }
}
