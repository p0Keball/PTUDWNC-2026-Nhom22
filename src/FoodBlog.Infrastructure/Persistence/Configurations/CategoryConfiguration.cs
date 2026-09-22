using FoodBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodBlog.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        BaseEntityConfig.Configure(builder);
        builder.ToTable("Categories");
        builder.Property(c => c.Name)
            .HasColumnType("varchar(100)")
            .IsRequired();
        builder.HasIndex(c => c.Name).IsUnique();
        builder.Property(c => c.Slug)
            .HasColumnType("varchar(120)")
            .IsRequired();
        builder.HasIndex(c => c.Slug).IsUnique();
        builder.Property(c => c.Description).HasColumnType("text");
        builder.Property(c => c.ImageUrl).HasColumnType("varchar(500)");
        builder.Property(c => c.OrderIndex).HasDefaultValue(0);
        builder.HasMany(c => c.Recipes)
            .WithOne(r => r.Category)
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
