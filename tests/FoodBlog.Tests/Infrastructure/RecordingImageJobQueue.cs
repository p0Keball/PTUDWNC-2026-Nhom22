using System.Collections.Concurrent;
using FoodBlog.Application.Interfaces;

namespace FoodBlog.Tests.Infrastructure;

/// <summary>Ghi nhận job thay vì chạy worker nền — dùng để assert endpoint có enqueue thumbnail/cleanup.</summary>
public sealed class RecordingImageJobQueue : IImageJobQueue
{
    public ConcurrentQueue<Guid> ThumbnailJobs { get; } = new();
    public ConcurrentQueue<IReadOnlyList<string>> CleanupJobs { get; } = new();

    public void EnqueueThumbnail(Guid imageId) => ThumbnailJobs.Enqueue(imageId);

    public void EnqueueCleanup(IReadOnlyList<string> imageUrls)
    {
        if (imageUrls.Count > 0)
            CleanupJobs.Enqueue(imageUrls);
    }
}
