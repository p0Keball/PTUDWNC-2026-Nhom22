using FoodBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.Application.Common.Interfaces;

public interface IFoodBlogDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Recipe> Recipes { get; }
    DbSet<ApplicationUser> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
