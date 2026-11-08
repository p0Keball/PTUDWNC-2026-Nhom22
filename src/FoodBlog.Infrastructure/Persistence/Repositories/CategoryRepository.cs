using FoodBlog.Application.Contracts.Persistence;
using FoodBlog.Domain.Entities;
using FoodBlog.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.Infrastructure.Persistence.Repositories;

public class CategoryRepository(FoodBlogDbContext db) : Repository<Category>(db), ICategoryRepository
{
    public Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        Db.Categories.FirstOrDefaultAsync(c => c.Slug == slug, ct);

    public Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default) =>
        excludeId.HasValue
            ? Db.Categories.AnyAsync(c => c.Id != excludeId && c.Name == name, ct)
            : Db.Categories.AnyAsync(c => c.Name == name, ct);

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct = default) =>
        Db.Categories.AnyAsync(c => c.Slug == slug, ct);

    public Task<List<Category>> ListOrderedWithRecipesAsync(CancellationToken ct = default) =>
        Db.Categories.Include(c => c.Recipes).OrderBy(c => c.Name).ToListAsync(ct);

    public Task<int> CountPublishedRecipesAsync(Guid categoryId, CancellationToken ct = default) =>
        Db.Recipes.CountAsync(r => r.CategoryId == categoryId && r.Status == RecipeStatus.Published, ct);

    public Task<int> CountAllRecipesAsync(Guid categoryId, CancellationToken ct = default) =>
        Db.Recipes.CountAsync(r => r.CategoryId == categoryId, ct);

    public async Task<(List<Recipe> Items, int TotalCount)> GetVisibleRecipesAsync(
        Guid categoryId,
        int page,
        int pageSize,
        string? currentUserId,
        bool isAdmin,
        CancellationToken ct = default)
    {
        var query = Db.Recipes
            .Include(r => r.Images)
            .Where(r => r.CategoryId == categoryId);

        // FR-CAT-002: Guest chỉ thấy Published; Author thấy thêm Draft/Archived
        // của chính mình; Admin thấy tất cả.
        if (!isAdmin)
        {
            if (!string.IsNullOrWhiteSpace(currentUserId))
            {
                var userId = currentUserId;
                query = query.Where(r =>
                    r.Status == RecipeStatus.Published ||
                    (r.Status != RecipeStatus.Published && r.AuthorId == userId));
            }
            else
            {
                query = query.Where(r => r.Status == RecipeStatus.Published);
            }
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }
}
