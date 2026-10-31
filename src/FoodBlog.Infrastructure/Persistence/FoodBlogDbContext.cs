using FoodBlog.Application.Common.Interfaces;
using FoodBlog.Domain.Common;
using FoodBlog.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FoodBlog.Infrastructure.Persistence;

public class FoodBlogDbContext : IdentityDbContext<ApplicationUser>, IFoodBlogDbContext
{
    public FoodBlogDbContext(DbContextOptions<FoodBlogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeStep> RecipeSteps => Set<RecipeStep>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<RecipeImage> RecipeImages => Set<RecipeImage>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var isPostgres = Database.IsNpgsql();
        if (isPostgres)
        {
            modelBuilder.HasPostgresExtension("unaccent");
            modelBuilder.HasPostgresExtension("pg_trgm");
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FoodBlogDbContext).Assembly);

        // SearchVector là shadow property kiểu NpgsqlTsVector, chỉ PostgreSQL mới map được.
        // Các provider dùng cho test (InMemory) cần bỏ property + index GIN tương ứng.
        if (!isPostgres)
        {
            var recipe = modelBuilder.Entity<Recipe>();
            var searchVectorIndex = recipe.Metadata.GetIndexes()
                .FirstOrDefault(i => i.Properties.Any(p => p.Name == "SearchVector"));
            if (searchVectorIndex is not null)
                recipe.Metadata.RemoveIndex((IMutableIndex)searchVectorIndex);
            recipe.Ignore("SearchVector");
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == default)
                    entry.Entity.CreatedAt = now;
                entry.Entity.RowVersion = Guid.NewGuid().ToByteArray();
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.RowVersion = Guid.NewGuid().ToByteArray();
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
