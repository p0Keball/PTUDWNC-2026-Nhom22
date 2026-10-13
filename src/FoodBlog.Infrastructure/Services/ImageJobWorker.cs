using System.Threading.Channels;
using FoodBlog.Application.Common;
using FoodBlog.Application.Interfaces;
using FoodBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;

namespace FoodBlog.Infrastructure.Services;

internal sealed record ImageJob(Guid? ImageId, IReadOnlyList<string>? ImageUrls)
{
    public bool IsThumbnail => ImageId.HasValue;
}

/// <summary>
/// Hàng đợi + worker chạy nền cho thumbnail và MinIO cleanup (FR-JOB-002).
/// Dùng Channel in-process; thay bằng Hangfire khi Long tích hợp xong mà không
/// đổi interface (IImageJobQueue).
/// </summary>
public sealed class ImageJobQueue : IImageJobQueue
{
    private readonly Channel<ImageJob> _channel = Channel.CreateUnbounded<ImageJob>();

    internal ChannelReader<ImageJob> Reader => _channel.Reader;

    public void EnqueueThumbnail(Guid imageId)
        => _channel.Writer.TryWrite(new ImageJob(imageId, null));

    public void EnqueueCleanup(IReadOnlyList<string> imageUrls)
    {
        if (imageUrls.Count > 0)
            _channel.Writer.TryWrite(new ImageJob(null, imageUrls));
    }
}

public sealed class ImageJobWorker(
    ImageJobQueue queue,
    IServiceScopeFactory scopes,
    ILogger<ImageJobWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                if (job.IsThumbnail)
                    await RunWithRetryAsync(() => ProcessThumbnailAsync(job.ImageId!.Value, stoppingToken), "thumbnail");
                else
                    await RunWithRetryAsync(() => ProcessCleanupAsync(job.ImageUrls!, stoppingToken), "cleanup");
            }
            catch (Exception ex)
            {
                // FR-JOB-002: sau 3 lần retry vẫn fail — ảnh gốc vẫn hiển thị, chỉ thiếu thumbnail.
                logger.LogError(ex, "Image job thất bại sau 3 lần retry.");
            }
        }
    }

    private async Task ProcessThumbnailAsync(Guid imageId, CancellationToken ct)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FoodBlogDbContext>();
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();
        var resizer = scope.ServiceProvider.GetRequiredService<IImageResizer>();

        var image = await db.RecipeImages.FirstOrDefaultAsync(i => i.Id == imageId, ct);
        if (image is null)
            return; // Ảnh đã bị xóa trước khi job chạy.
        if (image.ThumbnailUrl is not null && image.MediumUrl is not null)
            return; // Job trùng — idempotent.

        var originalKey = storage.TryExtractKey(image.OriginalUrl);
        if (originalKey is null)
            return;

        var (content, _) = await storage.DownloadAsync(originalKey, ct);
        await using var _ = content;
        ResizedImages resized;
        try
        {
            resized = await resizer.ResizeAsync(content, ct);
        }
        catch (UnknownImageFormatException)
        {
            // AVIF/không decode được — giữ nguyên pending, không retry vô ích.
            logger.LogWarning("Không decode được ảnh {ImageId}, bỏ qua thumbnail.", imageId);
            return;
        }
        await using var __ = resized.Medium;
        await using var ___ = resized.Thumbnail;

        var mediumKey = ImageVariants.MediumKey(originalKey);
        var thumbKey = ImageVariants.ThumbKey(originalKey);
        var mediumUrl = await storage.UploadStreamAsync(resized.Medium, mediumKey, "image/jpeg", ct);
        var thumbUrl = await storage.UploadStreamAsync(resized.Thumbnail, thumbKey, "image/jpeg", ct);

        image.SetThumbnails(mediumUrl, thumbUrl);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Đã tạo thumbnail cho ảnh {ImageId}.", imageId);
    }

    private async Task ProcessCleanupAsync(IReadOnlyList<string> imageUrls, CancellationToken ct)
    {
        using var scope = scopes.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();
        foreach (var url in imageUrls)
        {
            try
            {
                await storage.DeleteImageVariantsAsync(url, ct);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Xóa MinIO cleanup thất bại cho {Url}.", url);
                throw;
            }
        }
    }

    private static async Task RunWithRetryAsync(Func<Task> action, string jobName, int maxAttempts = 3)
    {
        var delay = TimeSpan.FromSeconds(5);
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await action();
                return;
            }
            catch when (attempt < maxAttempts)
            {
                await Task.Delay(delay);
                delay *= 2;
            }
        }
    }
}
