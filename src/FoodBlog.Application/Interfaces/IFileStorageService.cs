using Microsoft.AspNetCore.Http;

namespace FoodBlog.Application.Interfaces;

public sealed record PresignedUploadResult(string UploadUrl, string ObjectKey, string PublicUrl, DateTime ExpiresAt);

public sealed record StoredFileMetadata(long Size, string ContentType);

public interface IFileStorageService
{
    Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct = default);
    Task DeleteAsync(string fileUrl, CancellationToken ct = default);
    Task DeleteByKeyAsync(string objectKey, CancellationToken ct = default);

    /// <summary>Tạo presigned PUT URL để FE upload trực tiếp lên MinIO (không qua BE).</summary>
    Task<PresignedUploadResult> GetPresignedPutUrlAsync(
        string objectKey, string contentType, TimeSpan expiresIn, CancellationToken ct = default);

    string GetPublicUrl(string objectKey);

    Task<StoredFileMetadata?> GetMetadataAsync(string objectKey, CancellationToken ct = default);

    /// <summary>Đọc N byte đầu của object để kiểm tra magic bytes.</summary>
    Task<byte[]> ReadLeadingBytesAsync(string objectKey, int count, CancellationToken ct = default);

    /// <summary>Tải toàn bộ object về MemoryStream (tối đa file ảnh 5MB).</summary>
    Task<(Stream Content, string ContentType)> DownloadAsync(string objectKey, CancellationToken ct = default);

    /// <summary>Upload stream có sẵn lên object key chỉ định, trả về public URL.</summary>
    Task<string> UploadStreamAsync(Stream content, string objectKey, string contentType, CancellationToken ct = default);

    /// <summary>Trích xuất object key từ public URL (null khi không parse được).</summary>
    string? TryExtractKey(string fileUrl);

    /// <summary>
    /// Xóa đệ quy ảnh gốc + các biến thể medium/thumb (MinIO cleanup).
    /// Idempotent, best-effort từng key.
    /// </summary>
    Task DeleteImageVariantsAsync(string originalUrl, CancellationToken ct = default);

    Task EnsureBucketExistsAsync(CancellationToken ct = default);
}
