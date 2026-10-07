using FoodBlog.Domain.Entities;

namespace FoodBlog.Domain.Interfaces.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);
}