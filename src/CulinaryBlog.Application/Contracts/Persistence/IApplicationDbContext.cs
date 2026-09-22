namespace CulinaryBlog.Application.Contracts.Persistence;

using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public interface IApplicationDbContext
{
    DbSet<Recipe> Recipes { get; }
    DbSet<Category> Categories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}