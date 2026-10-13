namespace FoodBlog.Application.Interfaces;

/// <summary>Hàng đợi job ảnh chạy nền (FR-JOB-002, MinIO cleanup).</summary>
public interface IImageJobQueue
{
    void EnqueueThumbnail(Guid imageId);
    void EnqueueCleanup(IReadOnlyList<string> imageUrls);
}
