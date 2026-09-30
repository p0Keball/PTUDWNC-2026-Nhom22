using FoodBlog.Application.Contracts.Persistence;
using FoodBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.Infrastructure.Persistence.Repositories;

public class RecipeRepository(FoodBlogDbContext db) : Repository<Recipe>(db), IRecipeRepository
{
    public async Task<Recipe?> GetForUpdateAsync(Guid id, CancellationToken ct = default)
        => await DbSet
            .Include(r => r.Steps)
            .Include(r => r.Ingredients)
            .Include(r => r.Images)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default)
        => await DbSet.AnyAsync(r => r.Slug == slug, ct);
}
