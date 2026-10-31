using Amazon.S3;
using FoodBlog.Application.Options;
using FoodBlog.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FoodBlog.Tests.Storage;

public sealed class MinioFileStorageServiceTests
{
    private static MinioFileStorageService CreateService(
        string bucket = "culinary-blog", string publicUrl = "http://localhost:9000/")
        => new(Mock.Of<IAmazonS3>(), Options.Create(new MinioOptions
        {
            Endpoint = "localhost:9000",
            Bucket = bucket,
            PublicUrl = publicUrl,
        }));

    [Fact]
    public void TryExtractKey_ParsesKeyFromPublicUrl()
    {
        var service = CreateService();

        var key = service.TryExtractKey("http://localhost:9000/culinary-blog/recipes/abc/photo.jpg?x=1");

        Assert.Equal("recipes/abc/photo.jpg", key);
    }

    [Fact]
    public void TryExtractKey_RejectsUrlWithoutBucket()
        => Assert.Null(CreateService().TryExtractKey("https://cdn.example.com/photo.jpg"));

    [Fact]
    public void TryExtractKey_RejectsPathTraversal()
        => Assert.Null(CreateService().TryExtractKey("http://localhost:9000/culinary-blog/../etc/passwd"));

    [Fact]
    public void TryExtractKey_ReturnsNullForEmpty()
    {
        var service = CreateService();
        Assert.Null(service.TryExtractKey(""));
        Assert.Null(service.TryExtractKey("   "));
    }

    [Fact]
    public void GetPublicUrl_JoinsPublicBaseAndBucket()
    {
        var service = CreateService(bucket: "foodblog", publicUrl: "http://minio.local/");

        Assert.Equal("http://minio.local/foodblog/recipes/a.jpg", service.GetPublicUrl("recipes/a.jpg"));
    }

    [Fact]
    public async Task IsHealthyAsync_WhenStorageUnreachable_ReturnsFalseInsteadOfThrowing()
    {
        // Trỏ vào cổng không có service: client ném lỗi kết nối.
        // Contract của service: nuốt lỗi và trả false (graceful fallback).
        using var client = new AmazonS3Client("key", "secret", new AmazonS3Config
        {
            ServiceURL = "http://127.0.0.1:1",
            ForcePathStyle = true,
            MaxErrorRetry = 0,
            Timeout = TimeSpan.FromMilliseconds(500),
        });
        var service = new MinioFileStorageService(client, Options.Create(new MinioOptions
        {
            Endpoint = "127.0.0.1:1",
            Bucket = "culinary-blog",
            PublicUrl = "http://127.0.0.1:1",
        }));

        var healthy = await service.IsHealthyAsync();

        Assert.False(healthy);
    }
}
