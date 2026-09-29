using FoodBlog.Domain.Entities;
using FoodBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<FoodBlogDbContext>(o =>
            o.UseNpgsql(config.GetConnectionString("Default")));

        services.AddIdentityCore<ApplicationUser>(o =>
            {
                o.Password.RequiredLength = 8;
                o.Password.RequireDigit = true;
                o.Password.RequireUppercase = true;
                o.Password.RequireLowercase = true;
                o.Password.RequireNonAlphanumeric = true;
                o.User.RequireUniqueEmail = true;
                o.Lockout.AllowedForNewUsers = true;
                o.Lockout.MaxFailedAccessAttempts = 5;
                o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<FoodBlogDbContext>();

        // NFR-SEC-001: PBKDF2 >= 100.000 vòng lặp
        services.Configure<PasswordHasherOptions>(o => o.IterationCount = 100_000);

        services.AddSingleton(TimeProvider.System);
        return services;
    }
}