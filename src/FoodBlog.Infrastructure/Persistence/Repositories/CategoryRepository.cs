using FoodBlog.Application.Common.Interfaces;
using FoodBlog.Domain.Entities;
using FoodBlog.Domain.Enums;
using FoodBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.Infrastructure.Persistence.Repositories;

public class CategoryRepository(FoodBlogDbContext db) : GenericRepository<Category>(db), ICategoryRepository
{
    public Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        Db.Categories.FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);

    public Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default) =>
        excludeId.HasValue
            ? Db.Categories.AnyAsync(c => c.Id != excludeId && c.Name == name, cancellationToken)
            : Db.Categories.AnyAsync(c => c.Name == name, cancellationToken);

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        Db.Categories.AnyAsync(c => c.Slug == slug, cancellationToken);

    public Task<List<Category>> ListOrderedWithRecipesAsync(CancellationToken cancellationToken = default) =>
        Db.Categories.Include(c => c.Recipes).OrderBy(c => c.Name).ToListAsync(cancellationToken);

    public Task<int> CountPublishedRecipesAsync(Guid categoryId, CancellationToken cancellationToken = default) =>
        Db.Recipes.CountAsync(r => r.CategoryId == categoryId && r.Status == RecipeStatus.Published, cancellationToken);

    public Task<int> CountAllRecipesAsync(Guid categoryId, CancellationToken cancellationToken = default) =>
        Db.Recipes.CountAsync(r => r.CategoryId == categoryId, cancellationToken);

    public async Task<(List<Recipe> Items, int TotalCount)> GetPublishedRecipesAsync(Guid categoryId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = Db.Recipes
            .Include(r => r.Images)
            .Where(r => r.CategoryId == categoryId && r.Status == RecipeStatus.Published);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, total);
    }
}
