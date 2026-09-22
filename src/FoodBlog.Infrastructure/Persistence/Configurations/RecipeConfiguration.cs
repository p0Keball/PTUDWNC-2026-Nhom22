using FoodBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using NpgsqlTypes;

namespace FoodBlog.Infrastructure.Persistence.Configurations;

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        BaseEntityConfig.Configure(builder);
        builder.ToTable("Recipes");
        builder.Property(r => r.Title)
            .HasColumnType("varchar(200)")
            .IsRequired();
        builder.Property(r => r.Slug)
            .HasColumnType("varchar(220)")
            .IsRequired();
        builder.HasIndex(r => r.Slug).IsUnique();
        builder.Property(r => r.Description).HasColumnType("text").IsRequired();
        builder.Property(r => r.Instructions).HasColumnType("text").IsRequired();
        builder.Property(r => r.Difficulty).HasConversion<short>();
        builder.Property(r => r.Status).HasConversion<short>();
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.Difficulty);
        builder.Property(r => r.PublishedAt).HasColumnType("timestamptz");

        builder.OwnsOne(r => r.Nutrition, n =>
        {
            n.Property(x => x.Calories).HasColumnType("decimal(8,2)");
            n.Property(x => x.Protein).HasColumnType("decimal(8,2)");
            n.Property(x => x.Carbohydrates).HasColumnType("decimal(8,2)");
            n.Property(x => x.Fat).HasColumnType("decimal(8,2)");
            n.Property(x => x.Fiber).HasColumnType("decimal(8,2)");
            n.Property(x => x.Sodium).HasColumnType("decimal(8,2)");
        });

        builder.Property<NpgsqlTsVector>("SearchVector").HasColumnType("tsvector");
        builder.HasIndex("SearchVector").HasMethod("GIN");

        builder.HasOne(r => r.Author)
            .WithMany(u => u.Recipes)
            .HasForeignKey(r => r.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(r => r.Steps)
            .WithOne(s => s.Recipe)
            .HasForeignKey(s => s.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(r => r.Ingredients)
            .WithOne(i => i.Recipe)
            .HasForeignKey(i => i.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(r => r.Images)
            .WithOne(i => i.Recipe)
            .HasForeignKey(i => i.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
