using FoodBlog.Application.Common;
using FoodBlog.Application.Interfaces;
using FoodBlog.Domain.Entities;
using FoodBlog.Infrastructure.Persistence;
using FoodBlog.Infrastructure.Services;
using FoodBlog.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FoodBlog.Tests.Storage;

public sealed class ImageJobWorkerTests
{
    [Fact]
    public async Task ThumbnailJob_GeneratesVariantsAndPersistsUrls()
    {
        var storage = new InMemoryFileStorage();
        var databaseName = $"thumbnail-worker-{Guid.NewGuid():N}";

        var services = new ServiceCollection();
        services.AddDbContext<FoodBlogDbContext>(options => options.UseInMemoryDatabase(databaseName));
        services.AddSingleton<IFileStorageService>(storage);
        services.AddSingleton<IImageResizer, ImageSharpResizer>();
        services.AddSingleton<ImageJobQueue>();

        await using var provider = services.BuildServiceProvider();
        var queue = provider.GetRequiredService<ImageJobQueue>();
        var worker = new ImageJobWorker(
            queue,
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<ImageJobWorker>.Instance);

        var recipeId = Guid.NewGuid();
        var originalKey = $"recipes/{recipeId}/{Guid.NewGuid():N}.jpg";
        storage.PutObject(originalKey, TestImages.Jpeg(1600, 1200), "image/jpeg");

        Guid imageId;
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<FoodBlogDbContext>();
            var image = RecipeImage.Create(
                recipeId, storage.PublicUrlFor(originalKey), null, null, "Ảnh test", true, 0);
            db.RecipeImages.Add(image);
            await db.SaveChangesAsync();
            imageId = image.Id;
        }

        await worker.StartAsync(CancellationToken.None);
        try
        {
            queue.EnqueueThumbnail(imageId);
            var processed = await WaitUntilProcessedAsync(provider, imageId);
            Assert.NotNull(processed);

            Assert.True(storage.Contains(ImageVariants.MediumKey(originalKey)));
            Assert.True(storage.Contains(ImageVariants.ThumbKey(originalKey)));
            Assert.Equal(storage.PublicUrlFor(ImageVariants.MediumKey(originalKey)), processed!.MediumUrl);
            Assert.Equal(storage.PublicUrlFor(ImageVariants.ThumbKey(originalKey)), processed.ThumbnailUrl);

            var (medium, _) = await storage.DownloadAsync(ImageVariants.MediumKey(originalKey));
            await using var mediumStream = medium;
            var (thumb, _) = await storage.DownloadAsync(ImageVariants.ThumbKey(originalKey));
            await using var thumbStream = thumb;

            using var mediumImage = await SixLabors.ImageSharp.Image.LoadAsync(mediumStream);
            using var thumbImage = await SixLabors.ImageSharp.Image.LoadAsync(thumbStream);
            Assert.Equal(800, mediumImage.Width);
            Assert.Equal(600, mediumImage.Height);
            Assert.Equal(300, thumbImage.Width);
            Assert.Equal(300, thumbImage.Height);
        }
        finally
        {
            await worker.StopAsync(CancellationToken.None);
            worker.Dispose();
        }
    }

    [Fact]
    public async Task ThumbnailJob_ImageDeletedBeforeProcessing_DoesNothing()
    {
        var storage = new InMemoryFileStorage();
        var databaseName = $"thumbnail-worker-{Guid.NewGuid():N}";

        var services = new ServiceCollection();
        services.AddDbContext<FoodBlogDbContext>(options => options.UseInMemoryDatabase(databaseName));
        services.AddSingleton<IFileStorageService>(storage);
        services.AddSingleton<IImageResizer, ImageSharpResizer>();
        services.AddSingleton<ImageJobQueue>();

        await using var provider = services.BuildServiceProvider();
        var queue = provider.GetRequiredService<ImageJobQueue>();
        var worker = new ImageJobWorker(
            queue,
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<ImageJobWorker>.Instance);

        await worker.StartAsync(CancellationToken.None);
        try
        {
            queue.EnqueueThumbnail(Guid.NewGuid());
            await Task.Delay(200);
            Assert.Empty(storage.Keys);
        }
        finally
        {
            await worker.StopAsync(CancellationToken.None);
            worker.Dispose();
        }
    }

    private static async Task<RecipeImage?> WaitUntilProcessedAsync(IServiceProvider provider, Guid imageId)
    {
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FoodBlogDbContext>();
            var image = await db.RecipeImages.AsNoTracking().FirstOrDefaultAsync(i => i.Id == imageId);
            if (image?.ThumbnailUrl is not null && image.MediumUrl is not null)
                return image;
            await Task.Delay(100);
        }
        return null;
    }
}
