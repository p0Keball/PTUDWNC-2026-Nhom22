using FoodBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodBlog.Infrastructure.Persistence.Configurations;

public class RecipeStepConfiguration : IEntityTypeConfiguration<RecipeStep>
{
    public void Configure(EntityTypeBuilder<RecipeStep> builder)
    {
        BaseEntityConfig.Configure(builder);
        builder.ToTable("RecipeSteps");
        builder.Property(s => s.StepNumber).IsRequired();
        builder.Property(s => s.Title)
            .HasColumnType("varchar(200)")
            .IsRequired();
        builder.Property(s => s.Description).HasColumnType("text").IsRequired();
        builder.Property(s => s.ImageUrl).HasColumnType("varchar(500)");
        builder.HasIndex(s => new { s.RecipeId, s.StepNumber });
    }
}
