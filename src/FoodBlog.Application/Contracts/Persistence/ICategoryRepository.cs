using FoodBlog.Domain.Entities;

namespace FoodBlog.Application.Contracts.Persistence;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
    Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct = default);
    Task<List<Category>> ListOrderedWithRecipesAsync(CancellationToken ct = default);
    Task<int> CountPublishedRecipesAsync(Guid categoryId, CancellationToken ct = default);
    Task<int> CountAllRecipesAsync(Guid categoryId, CancellationToken ct = default);
    Task<(List<Recipe> Items, int TotalCount)> GetPublishedRecipesAsync(Guid categoryId, int page, int pageSize, CancellationToken ct = default);
}
