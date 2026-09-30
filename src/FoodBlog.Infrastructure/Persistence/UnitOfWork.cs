using FoodBlog.Application.Contracts.Persistence;
using FoodBlog.Domain.Entities;
using FoodBlog.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace FoodBlog.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly FoodBlogDbContext _db;
    private IDbContextTransaction? _transaction;

    private IRecipeRepository? _recipes;
    private IRepository<Category>? _categories;

    public UnitOfWork(FoodBlogDbContext db) => _db = db;

    public IRecipeRepository Recipes
        => _recipes ??= new RecipeRepository(_db);

    public IRepository<Category> Categories
        => _categories ??= new Repository<Category>(_db);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await _db.Database.BeginTransactionAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(ct);
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(ct);
            _transaction = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
            await _transaction.DisposeAsync();
        await _db.DisposeAsync();
    }
}
