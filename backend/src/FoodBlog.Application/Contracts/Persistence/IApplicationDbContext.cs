using FoodBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.Application.Contracts.Persistence;

/// <summary>
/// Abstraction cho DbContext (DIP §1.2.3).
/// Application chỉ phụ thuộc interface này, Infrastructure implement bằng EF Core.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Recipe> Recipes { get; }
    DbSet<RecipeStep> RecipeSteps { get; }
    DbSet<RecipeIngredient> RecipeIngredients { get; }
    DbSet<RecipeImage> RecipeImages { get; }
    DbSet<ApplicationUser> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
