using System.Net.Sockets;
using Amazon.S3;
using FoodBlog.Application.Options;
using FoodBlog.Infrastructure.Services;
using FoodBlog.Tests.Infrastructure;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Abstractions;

namespace FoodBlog.Tests.Storage;

/// <summary>
/// Test thật với MinIO (docker compose up -d minio).
/// Nếu MinIO không chạy, test tự bỏ qua để `dotnet test` vẫn xanh trên máy sạch.
/// Có thể override bằng biến môi trường:
/// FOODBLOG_MINIO_ENDPOINT / FOODBLOG_MINIO_ACCESSKEY / FOODBLOG_MINIO_SECRETKEY.
/// </summary>
[Trait("Category", "RequiresMinIO")]
public sealed class MinioIntegrationTests
{
    private readonly ITestOutputHelper _output;

    public MinioIntegrationTests(ITestOutputHelper output) => _output = output;

    private static string Endpoint => Environment.GetEnvironmentVariable("FOODBLOG_MINIO_ENDPOINT") ?? "localhost:9000";
    private static string AccessKey => Environment.GetEnvironmentVariable("FOODBLOG_MINIO_ACCESSKEY") ?? "minioadmin";
    private static string SecretKey => Environment.GetEnvironmentVariable("FOODBLOG_MINIO_SECRETKEY") ?? "minioadmin123";

    private static async Task<bool> IsMinioReachableAsync()
    {
        try
        {
            var parts = Endpoint.Split(':');
            using var client = new TcpClient();
            await client.ConnectAsync(parts[0], parts.Length > 1 ? int.Parse(parts[1]) : 9000)
                .WaitAsync(TimeSpan.FromSeconds(1));
            return client.Connected;
        }
        catch
        {
            return false;
        }
    }

    private static (MinioFileStorageService Service, IAmazonS3 Client) CreateService(string bucket)
    {
        var options = new MinioOptions
        {
            Endpoint = Endpoint,
            AccessKey = AccessKey,
            SecretKey = SecretKey,
            Bucket = bucket,
            PublicUrl = $"http://{Endpoint}",
            UseSsl = false,
        };
        var client = new AmazonS3Client(AccessKey, SecretKey, new AmazonS3Config
        {
            ServiceURL = $"http://{Endpoint}",
            ForcePathStyle = true,
        });
        return (new MinioFileStorageService(client, Options.Create(options)), client);
    }

    [Fact]
    public async Task UploadReadPresignAndDelete_AgainstRealMinio()
    {
        if (!await IsMinioReachableAsync())
        {
            _output.WriteLine($"MinIO không chạy tại {Endpoint} — bỏ qua test tích hợp.");
            return;
        }

        var bucket = $"foodblog-tests-{Guid.NewGuid():N}";
        var (service, client) = CreateService(bucket);
        using (client)
        {
            var key = $"recipes/{Guid.NewGuid():N}/sample.jpg";

            await service.EnsureBucketExistsAsync();
            await service.UploadStreamAsync(new MemoryStream(TestImages.Jpeg()), key, "image/jpeg");

            var metadata = await service.GetMetadataAsync(key);
            Assert.NotNull(metadata);
            Assert.True(metadata!.Size > 0);
            Assert.Equal("image/jpeg", metadata.ContentType);

            var leading = await service.ReadLeadingBytesAsync(key, 4);
            Assert.Equal([0xFF, 0xD8, 0xFF, 0xE0], leading);

            var publicUrl = service.GetPublicUrl(key);
            Assert.Equal(key, service.TryExtractKey(publicUrl));

            var presigned = await service.GetPresignedPutUrlAsync(
                key, "image/jpeg", TimeSpan.FromMinutes(5));
            Assert.Contains("X-Amz-Signature", presigned.UploadUrl);
            Assert.Equal(publicUrl, presigned.PublicUrl);

            await service.DeleteImageVariantsAsync(publicUrl);
            Assert.Null(await service.GetMetadataAsync(key));
        }
    }
}
