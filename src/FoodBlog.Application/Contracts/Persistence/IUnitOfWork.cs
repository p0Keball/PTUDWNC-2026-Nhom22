using FoodBlog.Domain.Entities;

namespace FoodBlog.Application.Contracts.Persistence;

public interface IUnitOfWork : IAsyncDisposable
{
    IRecipeRepository Recipes { get; }
    IRepository<Category> Categories { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
