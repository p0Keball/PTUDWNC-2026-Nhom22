using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using FoodBlog.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FoodBlog.Tests.Infrastructure;

/// <summary>
/// MinIO giả lập chạy trong bộ nhớ cho integration test:
/// cùng contract <see cref="IFileStorageService"/> nhưng không cần Docker.
/// </summary>
public sealed class InMemoryFileStorage : IFileStorageService
{
    public const string Bucket = "foodblog-tests";
    public const string PublicBaseUrl = "http://storage.test/" + Bucket;

    private readonly ConcurrentDictionary<string, StoredObject> _objects = new(StringComparer.Ordinal);

    /// <summary>Bật/tắt để test graceful fallback khi storage disconnect.</summary>
    public bool Healthy { get; set; } = true;

    public ConcurrentQueue<string> DeletedKeys { get; } = new();

    public IReadOnlyCollection<string> Keys => _objects.Keys.ToArray();

    public bool Contains(string key) => _objects.ContainsKey(key);

    public string PublicUrlFor(string key) => $"{PublicBaseUrl}/{key}";

    public void PutObject(string key, byte[] bytes, string contentType = "image/jpeg", DateTimeOffset? lastModified = null)
        => _objects[key] = new StoredObject(bytes, contentType, lastModified ?? DateTimeOffset.UtcNow);

    public Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var key = $"{folder.Trim('/')}/{Guid.NewGuid():N}{ext}";
        using var buffer = new MemoryStream();
        file.CopyTo(buffer);
        _objects[key] = new StoredObject(buffer.ToArray(), file.ContentType, DateTimeOffset.UtcNow);
        return Task.FromResult(PublicUrlFor(key));
    }

    public Task DeleteAsync(string fileUrl, CancellationToken ct = default)
    {
        var key = TryExtractKey(fileUrl);
        return key is null ? Task.CompletedTask : DeleteByKeyAsync(key, ct);
    }

    public Task DeleteByKeyAsync(string objectKey, CancellationToken ct = default)
    {
        _objects.TryRemove(objectKey, out _);
        DeletedKeys.Enqueue(objectKey);
        return Task.CompletedTask;
    }

    public Task<PresignedUploadResult> GetPresignedPutUrlAsync(
        string objectKey, string contentType, TimeSpan expiresIn, CancellationToken ct = default)
        => Task.FromResult(new PresignedUploadResult(
            $"http://storage.test/upload/{objectKey}?sig=test",
            objectKey,
            PublicUrlFor(objectKey),
            DateTime.UtcNow.Add(expiresIn)));

    public string GetPublicUrl(string objectKey) => PublicUrlFor(objectKey);

    public Task<StoredFileMetadata?> GetMetadataAsync(string objectKey, CancellationToken ct = default)
        => Task.FromResult(_objects.TryGetValue(objectKey, out var obj)
            ? new StoredFileMetadata(obj.Bytes.Length, obj.ContentType)
            : null);

    public Task<byte[]> ReadLeadingBytesAsync(string objectKey, int count, CancellationToken ct = default)
    {
        if (!_objects.TryGetValue(objectKey, out var obj))
            throw new FileNotFoundException("Object not found in fake storage.", objectKey);
        return Task.FromResult(obj.Bytes.Take(count).ToArray());
    }

    public Task<(Stream Content, string ContentType)> DownloadAsync(string objectKey, CancellationToken ct = default)
    {
        if (!_objects.TryGetValue(objectKey, out var obj))
            throw new FileNotFoundException("Object not found in fake storage.", objectKey);
        return Task.FromResult<(Stream, string)>((new MemoryStream(obj.Bytes, writable: false), obj.ContentType));
    }

    public Task<string> UploadStreamAsync(Stream content, string objectKey, string contentType, CancellationToken ct = default)
    {
        using var buffer = new MemoryStream();
        content.CopyTo(buffer);
        _objects[objectKey] = new StoredObject(buffer.ToArray(), contentType, DateTimeOffset.UtcNow);
        return Task.FromResult(PublicUrlFor(objectKey));
    }

    public string? TryExtractKey(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            return null;
        var prefix = $"{Bucket}/";
        var idx = fileUrl.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
            return null;
        var key = fileUrl[(idx + prefix.Length)..].Split('?')[0];
        return key.Contains("..") ? null : key;
    }

    public async Task DeleteImageVariantsAsync(string originalUrl, CancellationToken ct = default)
    {
        var key = TryExtractKey(originalUrl);
        if (key is null)
            return;
        foreach (var variant in FoodBlog.Application.Common.ImageVariants.AllKeys(key))
            await DeleteByKeyAsync(variant, ct);
    }

    public Task EnsureBucketExistsAsync(CancellationToken ct = default) => Task.CompletedTask;

    public Task<bool> IsHealthyAsync(CancellationToken ct = default) => Task.FromResult(Healthy);

    public async IAsyncEnumerable<StoredObjectInfo> ListKeysAsync(
        string prefix = "recipes/",
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        foreach (var (key, obj) in _objects.ToArray())
        {
            if (!key.StartsWith(prefix, StringComparison.Ordinal))
                continue;
            yield return new StoredObjectInfo(key, obj.Bytes.Length, obj.LastModified);
        }
        await Task.CompletedTask;
    }

    private sealed record StoredObject(byte[] Bytes, string ContentType, DateTimeOffset LastModified);
}
