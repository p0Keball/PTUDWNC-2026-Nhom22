namespace FoodBlog.Application.Interfaces;

/// <summary>Kết quả resize: stream JPEG của 2 biến thể (caller dispose).</summary>
public sealed record ResizedImages(Stream Medium, Stream Thumbnail);

/// <summary>Tạo biến thể medium 800×600 và thumbnail 300×300 (FR-JOB-002).</summary>
public interface IImageResizer
{
    Task<ResizedImages> ResizeAsync(Stream source, CancellationToken ct = default);
}
