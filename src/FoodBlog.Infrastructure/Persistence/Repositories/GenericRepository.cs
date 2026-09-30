using FoodBlog.Application.Common.Interfaces;
using FoodBlog.Domain.Common;
using FoodBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.Infrastructure.Persistence.Repositories;

public class GenericRepository<T>(FoodBlogDbContext db) : IRepository<T> where T : BaseEntity
{
    protected readonly FoodBlogDbContext Db = db;
    protected DbSet<T> Set => Db.Set<T>();

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Set.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<List<T>> ListAsync(CancellationToken cancellationToken = default) =>
        Set.ToListAsync(cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await Set.AddAsync(entity, cancellationToken);

    public void Remove(T entity) => Set.Remove(entity);
}
