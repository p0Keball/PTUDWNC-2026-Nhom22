using FoodBlog.Application.Interfaces;
using FoodBlog.Application.Options;
using FoodBlog.Domain.Entities;
using FoodBlog.Infrastructure.Persistence;
using FoodBlog.Infrastructure.Services;
using FoodBlog.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace FoodBlog.Tests.Storage;

public sealed class OrphanCleanupTests
{
    private sealed record Harness(
        ServiceProvider Provider,
        InMemoryFileStorage Storage,
        OrphanCleanupRunner Runner) : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => Provider.DisposeAsync();
    }

    private static async Task<Harness> CreateAsync()
    {
        var storage = new InMemoryFileStorage();
        var databaseName = $"orphan-{Guid.NewGuid():N}";

        var services = new ServiceCollection();
        services.AddDbContext<FoodBlogDbContext>(options => options.UseInMemoryDatabase(databaseName));
        services.AddSingleton<IFileStorageService>(storage);
        services.AddSingleton<IOptions<OrphanCleanupOptions>>(Options.Create(new OrphanCleanupOptions
        {
            Enabled = false,
            Prefix = "recipes/",
            MinAgeHours = 1,
        }));

        var provider = services.BuildServiceProvider();

        var referencedKey = $"recipes/{Guid.NewGuid():N}/kept.jpg";
        storage.PutObject(referencedKey, TestImages.Jpeg(40, 40));

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FoodBlogDbContext>();
            db.RecipeImages.Add(RecipeImage.Create(
                Guid.NewGuid(), storage.PublicUrlFor(referencedKey), null, null, null, true, 0));
            await db.SaveChangesAsync();
        }

        var runner = new OrphanCleanupRunner(
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<IOptions<OrphanCleanupOptions>>(),
            NullLogger<OrphanCleanupRunner>.Instance);

        return new Harness(provider, storage, runner);
    }

    [Fact]
    public async Task DryRun_ListsOnlyOldOrphans()
    {
        await using var harness = await CreateAsync();
        var orphanKey = $"recipes/{Guid.NewGuid():N}/orphan-old.jpg";
        var freshKey = $"recipes/{Guid.NewGuid():N}/presigned-new.jpg";
        harness.Storage.PutObject(orphanKey, TestImages.Jpeg(40, 40), lastModified: DateTimeOffset.UtcNow.AddHours(-5));
        harness.Storage.PutObject(freshKey, TestImages.Jpeg(40, 40), lastModified: DateTimeOffset.UtcNow);

        var report = await harness.Runner.ScanAsync(dryRun: true);

        Assert.Null(report.Error);
        Assert.Equal(3, report.TotalObjects);
        Assert.Equal(1, report.OrphanCount);
        Assert.Contains(orphanKey, report.OrphanKeys);
        Assert.DoesNotContain(freshKey, report.OrphanKeys);
        Assert.True(harness.Storage.Contains(orphanKey));
    }

    [Fact]
    public async Task Cleanup_DeletesOrphans_KeepsReferencedAndFreshObjects()
    {
        await using var harness = await CreateAsync();
        var referencedKey = harness.Storage.Keys.Single(k => k.EndsWith("kept.jpg"));
        var orphanKey = $"recipes/{Guid.NewGuid():N}/orphan-old.jpg";
        var freshKey = $"recipes/{Guid.NewGuid():N}/presigned-new.jpg";
        harness.Storage.PutObject(orphanKey, TestImages.Jpeg(40, 40), lastModified: DateTimeOffset.UtcNow.AddHours(-5));
        harness.Storage.PutObject(freshKey, TestImages.Jpeg(40, 40), lastModified: DateTimeOffset.UtcNow);

        var report = await harness.Runner.ScanAsync(dryRun: false);

        Assert.Equal(1, report.OrphanCount);
        Assert.False(harness.Storage.Contains(orphanKey));
        Assert.True(harness.Storage.Contains(referencedKey));
        Assert.True(harness.Storage.Contains(freshKey));
    }
}
