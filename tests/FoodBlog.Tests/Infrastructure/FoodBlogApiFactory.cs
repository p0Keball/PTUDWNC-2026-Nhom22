using Amazon.S3;
using FoodBlog.Application.Common.Interfaces;
using FoodBlog.Application.Contracts.Persistence;
using FoodBlog.Application.Interfaces;
using FoodBlog.Infrastructure.Persistence;
using FoodBlog.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace FoodBlog.Tests.Infrastructure;

/// <summary>
/// Boot API thật (Program.cs) nhưng thay hạ tầng bằng:
/// - EF Core InMemory (không cần PostgreSQL),
/// - <see cref="InMemoryFileStorage"/> (không cần MinIO),
/// - <see cref="RecordingImageJobQueue"/> (không chạy worker nền).
/// Nhờ vậy integration test child-data chạy được bằng `dotnet test` trên máy sạch.
/// </summary>
public sealed class FoodBlogApiFactory : WebApplicationFactory<Program>
{
    public string DatabaseName { get; } = $"foodblog-tests-{Guid.NewGuid():N}";

    public InMemoryFileStorage Storage { get; } = new();

    public RecordingImageJobQueue Jobs { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Host=unused;Database=unused;Username=unused;Password=unused",
                ["OrphanCleanup:Enabled"] = "false",
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Bỏ đăng ký Npgsql + DbContext của app để thay bằng InMemory.
            foreach (var descriptor in services
                         .Where(d => d.ServiceType.IsGenericType
                                     && d.ServiceType.GetGenericArguments().Contains(typeof(FoodBlogDbContext)))
                         .ToList())
            {
                services.Remove(descriptor);
            }
            services.RemoveAll<FoodBlogDbContext>();
            services.RemoveAll<IFoodBlogDbContext>();
            services.RemoveAll<IUnitOfWork>();

            services.AddDbContext<FoodBlogDbContext>(options =>
                options.UseInMemoryDatabase(DatabaseName));
            services.AddScoped<IFoodBlogDbContext>(sp => sp.GetRequiredService<FoodBlogDbContext>());
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Thay MinIO + job queue.
            services.RemoveAll<IFileStorageService>();
            services.AddSingleton<IFileStorageService>(Storage);
            services.RemoveAll<IImageJobQueue>();
            services.AddSingleton<IImageJobQueue>(Jobs);

            // Không khởi động hosted service nào trong test host (Hangfire server,
            // image worker, orphan cleanup) để DisposeAsync không chờ shutdown.
            services.RemoveAll<IHostedService>();

            // Integration test không chạy qua luồng đăng nhập thật: dùng scheme
            // giả lập để mọi request là user Author đã seed trong TestData.
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    options.DefaultScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });
        });
    }
}
