using Amazon.S3;
using FoodBlog.Application.Interfaces;
using FoodBlog.Application.Options;
using FoodBlog.Domain.Entities;
using FoodBlog.Infrastructure.Persistence;
using FoodBlog.Infrastructure.Services;
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
        services.Configure<PasswordHasherOptions>(o =>
        {
            o.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
            o.IterationCount = 100_000;
        });

        services.AddSingleton(TimeProvider.System);
        services.AddFileStorage(config);
        return services;
    }

    public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<MinioOptions>(config.GetSection(MinioOptions.SectionName));
        var minio = config.GetSection(MinioOptions.SectionName).Get<MinioOptions>() ?? new MinioOptions();

        services.AddSingleton<IAmazonS3>(_ =>
        {
            var s3Config = new AmazonS3Config
            {
                ServiceURL = $"{(minio.UseSsl ? "https" : "http")}://{minio.Endpoint}",
                ForcePathStyle = true,
            };
            return new AmazonS3Client(minio.AccessKey, minio.SecretKey, s3Config);
        });
        services.AddSingleton<IFileStorageService, MinioFileStorageService>();
        return services;
    }
}