using FoodBlog.Domain.Entities;

namespace FoodBlog.Domain.Interfaces.Repositories;

public interface IRecipeRepository : IRepository<Recipe>
{
    Task<Recipe?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);
}