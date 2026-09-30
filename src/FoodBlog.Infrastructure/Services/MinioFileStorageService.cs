using Amazon.S3;
using Amazon.S3.Transfer;
using FoodBlog.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FoodBlog.Infrastructure.Services;

public class MinioFileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName = "culinary-blog";

    public MinioFileStorageService(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    public async Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct = default)
    {
        // Tạo file name unique để tránh path traversal
        var ext = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{folder}/{Guid.NewGuid()}{ext}";

        using var newMemoryStream = new MemoryStream();
        await file.CopyToAsync(newMemoryStream, ct);

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = newMemoryStream,
            Key = uniqueFileName,
            BucketName = _bucketName,
            ContentType = file.ContentType
        };

        var fileTransferUtility = new TransferUtility(_s3Client);
        await fileTransferUtility.UploadAsync(uploadRequest, ct);

        // Giả sử endpoint MinIO là public-read
        return $"https://minio.yourdomain.com/{_bucketName}/{uniqueFileName}"; 
    }

    public Task DeleteAsync(string fileUrl, CancellationToken ct = default)
    {
        // Logic extract object name từ URL và gọi _s3Client.DeleteObjectAsync
        throw new NotImplementedException();
    }
}