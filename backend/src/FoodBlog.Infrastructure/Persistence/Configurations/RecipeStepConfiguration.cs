using FoodBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodBlog.Infrastructure.Persistence.Configurations;

public class RecipeStepConfiguration : IEntityTypeConfiguration<RecipeStep>
{
    public void Configure(EntityTypeBuilder<RecipeStep> builder)
    {
        builder.ToTable("RecipeSteps");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(s => s.ImageUrl)
            .HasMaxLength(500);

        // StepNumber duy nhất trong phạm vi 1 recipe (renumber khi xóa, FR-RCP-010)
        builder.HasIndex(s => new { s.RecipeId, s.StepNumber }).IsUnique();

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
