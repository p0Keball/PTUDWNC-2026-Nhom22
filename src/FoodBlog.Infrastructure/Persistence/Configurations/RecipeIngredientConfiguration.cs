using FoodBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodBlog.Infrastructure.Persistence.Configurations;

public class RecipeIngredientConfiguration : IEntityTypeConfiguration<RecipeIngredient>
{
    public void Configure(EntityTypeBuilder<RecipeIngredient> builder)
    {
        BaseEntityConfig.Configure(builder);
        builder.ToTable("RecipeIngredients");
        builder.Property(i => i.Name)
            .HasColumnType("varchar(200)")
            .IsRequired();
        builder.Property(i => i.Quantity).HasColumnType("decimal(10,3)");
        builder.Property(i => i.Unit).HasColumnType("varchar(50)");
        builder.Property(i => i.Notes).HasColumnType("varchar(500)");
        builder.Property(i => i.OrderIndex).HasDefaultValue(0);
        builder.HasIndex(i => new { i.RecipeId, i.OrderIndex });
    }
}
