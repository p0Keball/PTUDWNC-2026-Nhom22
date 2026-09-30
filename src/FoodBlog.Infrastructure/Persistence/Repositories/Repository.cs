using FoodBlog.Application.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.Infrastructure.Persistence.Repositories;

public class Repository<T>(FoodBlogDbContext db) : IRepository<T> where T : class
{
    protected readonly FoodBlogDbContext Db = db;
    protected readonly DbSet<T> DbSet = db.Set<T>();

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await DbSet.FindAsync([id], ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await DbSet.AsNoTracking().ToListAsync(ct);

    public IQueryable<T> Query() => DbSet.AsQueryable();

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await DbSet.AddAsync(entity, ct);

    public void Update(T entity) => DbSet.Update(entity);

    public void Remove(T entity) => DbSet.Remove(entity);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => await DbSet.FindAsync([id], ct) is not null;

    public async Task<int> CountAsync(CancellationToken ct = default)
        => await DbSet.CountAsync(ct);
}
