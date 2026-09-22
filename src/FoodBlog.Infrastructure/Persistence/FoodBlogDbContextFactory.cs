using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FoodBlog.Infrastructure.Persistence;

public class FoodBlogDbContextFactory : IDesignTimeDbContextFactory<FoodBlogDbContext>
{
    public FoodBlogDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../FoodBlog.API"))
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Default")
            ?? "Host=localhost;Port=5432;Database=foodblog;Username=foodblog;Password=foodblog123";

        var options = new DbContextOptionsBuilder<FoodBlogDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new FoodBlogDbContext(options);
    }
}
