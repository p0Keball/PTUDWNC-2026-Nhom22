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

    Task EnsureBucketExistsAsync(CancellationToken ct = default);
}
