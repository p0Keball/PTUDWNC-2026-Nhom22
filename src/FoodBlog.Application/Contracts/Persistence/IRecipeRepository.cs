using FoodBlog.Domain.Entities;

namespace FoodBlog.Application.Contracts.Persistence;

public interface IRecipeRepository : IRepository<Recipe>
{
    Task<Recipe?> GetForUpdateAsync(Guid id, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
}
