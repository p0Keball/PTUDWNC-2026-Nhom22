using FoodBlog.Domain.Entities;

namespace FoodBlog.Application.Common.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<Category>> ListOrderedWithRecipesAsync(CancellationToken cancellationToken = default);
    Task<int> CountPublishedRecipesAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<int> CountAllRecipesAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<(List<Recipe> Items, int TotalCount)> GetPublishedRecipesAsync(Guid categoryId, int page, int pageSize, CancellationToken cancellationToken = default);
}
