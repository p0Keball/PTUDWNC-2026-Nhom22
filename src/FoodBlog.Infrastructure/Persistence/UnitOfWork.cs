using FoodBlog.Application.Common.Interfaces;
using FoodBlog.Infrastructure.Persistence.Repositories;

namespace FoodBlog.Infrastructure.Persistence;

public class UnitOfWork(FoodBlogDbContext db) : IUnitOfWork
{
    public ICategoryRepository Categories { get; } = new CategoryRepository(db);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
