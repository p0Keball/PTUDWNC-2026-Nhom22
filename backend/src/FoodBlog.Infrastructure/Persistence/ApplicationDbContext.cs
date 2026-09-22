using FoodBlog.Application.Contracts.Persistence;
using FoodBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext implement IApplicationDbContext (§1.5.1).
/// Application không phụ thuộc EF Core trực tiếp, chỉ qua interface.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeStep> RecipeSteps => Set<RecipeStep>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Tự động áp dụng mọi IEntityTypeConfiguration trong assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
