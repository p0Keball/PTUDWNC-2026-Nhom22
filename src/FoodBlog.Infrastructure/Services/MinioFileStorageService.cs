using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using FoodBlog.Application.Interfaces;
using FoodBlog.Application.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace FoodBlog.Infrastructure.Services;

public class MinioFileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly MinioOptions _options;

    public MinioFileStorageService(IAmazonS3 s3Client, IOptions<MinioOptions> options)
    {
        _s3Client = s3Client;
        _options = options.Value;
    }

    public async Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var objectKey = $"{folder.Trim('/')}/{Guid.NewGuid():N}{ext}";

        await EnsureBucketExistsAsync(ct);

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream, ct);
        stream.Position = 0;

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = stream,
            Key = objectKey,
            BucketName = _options.Bucket,
            ContentType = file.ContentType,
        };

        var transferUtility = new TransferUtility(_s3Client);
        await transferUtility.UploadAsync(uploadRequest, ct);

        return GetPublicUrl(objectKey);
    }

    public Task DeleteAsync(string fileUrl, CancellationToken ct = default)
    {
        var key = ExtractKey(fileUrl);
        return key is null ? Task.CompletedTask : DeleteByKeyAsync(key, ct);
    }

    public async Task DeleteByKeyAsync(string objectKey, CancellationToken ct = default)
    {
        // FR-FILE-002: idempotent khi object không tồn tại, retry tối đa 3 lần.
        await ExecuteWithRetryAsync(async () =>
        {
            try
            {
                await _s3Client.DeleteObjectAsync(_options.Bucket, objectKey, ct);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Đã bị xóa trước đó — coi như thành công (idempotent).
            }
        }, ct);
    }

    public Task<PresignedUploadResult> GetPresignedPutUrlAsync(
        string objectKey, string contentType, TimeSpan expiresIn, CancellationToken ct = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _options.Bucket,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.Add(expiresIn),
            ContentType = contentType,
        };

        var uploadUrl = _s3Client.GetPreSignedURL(request);
        return Task.FromResult(new PresignedUploadResult(uploadUrl, objectKey, GetPublicUrl(objectKey),
            DateTime.UtcNow.Add(expiresIn)));
    }

    public string GetPublicUrl(string objectKey)
        => $"{_options.PublicUrl.TrimEnd('/')}/{_options.Bucket}/{objectKey}";

    public async Task<StoredFileMetadata?> GetMetadataAsync(string objectKey, CancellationToken ct = default)
    {
        try
        {
            var response = await _s3Client.GetObjectMetadataAsync(_options.Bucket, objectKey, ct);
            return new StoredFileMetadata(response.ContentLength, response.Headers.ContentType);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<byte[]> ReadLeadingBytesAsync(string objectKey, int count, CancellationToken ct = default)
    {
        var request = new GetObjectRequest
        {
            BucketName = _options.Bucket,
            Key = objectKey,
            ByteRange = new ByteRange(0, count - 1),
        };
        using var response = await _s3Client.GetObjectAsync(request, ct);
        using var ms = new MemoryStream();
        await response.ResponseStream.CopyToAsync(ms, ct);
        return ms.ToArray();
    }

    public async Task EnsureBucketExistsAsync(CancellationToken ct = default)
    {
        await ExecuteWithRetryAsync(async () =>
        {
            var exists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _options.Bucket);
            if (exists)
                return;

            await _s3Client.PutBucketAsync(_options.Bucket, ct);
        }, ct);

        // FR-FILE-001: bucket public-read cho ảnh công thức (best-effort).
        try
        {
            var policy = "{\"Version\":\"2012-10-17\",\"Statement\":[{\"Effect\":\"Allow\"," +
                "\"Principal\":{\"AWS\":[\"*\"]},\"Action\":[\"s3:GetObject\"]," +
                "\"Resource\":[\"arn:aws:s3:::" + _options.Bucket + "/*\"]}]}";
            // Đơn giản: bỏ qua lỗi policy (MinIO dev có thể chưa hỗ trợ đầy đủ).
            await _s3Client.PutBucketPolicyAsync(_options.Bucket, policy, ct);
        }
        catch
        {
            // Best-effort — không fail request vì policy.
        }
    }

    /// <summary>Retry tối đa 3 lần với backoff cho thao tác MinIO (FR-FILE-002).</summary>
    private static async Task ExecuteWithRetryAsync(Func<Task> action, CancellationToken ct, int maxAttempts = 3)
    {
        var delay = TimeSpan.FromMilliseconds(200);
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await action();
                return;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw;
            }
            catch when (attempt < maxAttempts)
            {
                await Task.Delay(delay, ct);
                delay *= 2;
            }
        }
    }

    private string? ExtractKey(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            return null;

        var prefix = $"{_options.Bucket}/";
        var idx = fileUrl.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
            return null;

        var key = fileUrl[(idx + prefix.Length)..].Split('?')[0];
        // Chống path traversal.
        if (key.Contains(".."))
            return null;
        return key;
    }
}
