using FoodBlog.Application.Common;
using FoodBlog.Application.Interfaces;
using FoodBlog.Application.Options;
using FoodBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodBlog.Infrastructure.Services;

public sealed record OrphanReport(
    int TotalObjects,
    int ReferencedObjects,
    int OrphanCount,
    long OrphanBytes,
    IReadOnlyList<string> OrphanKeys,
    DateTimeOffset CheckedAt,
    bool DryRun,
    string? Error = null);

/// <summary>
/// TUẦN 4 (Trần Gia Huy) — Orphaned Files: đối chiếu MinIO vs DB, xóa file mồ côi.
/// Orphan = object trên MinIO không còn DB row nào trỏ tới (presign abort,
/// confirm fail sót medium/thumb, xóa tay DB...). Bỏ qua object mới hơn
/// MinAgeHours để không xóa presigned vừa upload chưa confirm.
/// </summary>
public sealed class OrphanCleanupRunner(
    IServiceScopeFactory scopes,
    IOptions<OrphanCleanupOptions> options,
    ILogger<OrphanCleanupRunner> logger)
{
    private readonly OrphanCleanupOptions _options = options.Value;

    public async Task<OrphanReport> ScanAsync(
        bool dryRun = true,
        string? prefix = null,
        double? minAgeHours = null,
        int maxKeysInReport = 200,
        CancellationToken ct = default)
    {
        var checkedAt = DateTimeOffset.UtcNow;
        var effectivePrefix = prefix ?? _options.Prefix;
        var minAge = TimeSpan.FromHours(minAgeHours ?? _options.MinAgeHours);

        using var scope = scopes.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();
        var db = scope.ServiceProvider.GetRequiredService<FoodBlogDbContext>();

        // 1. Thu thập mọi key còn được DB tham chiếu.
        var referenced = new HashSet<string>(StringComparer.Ordinal);
        try
        {
            await foreach (var url in ReferencedUrlsAsync(db, ct))
            {
                var key = storage.TryExtractKey(url);
                if (key is null)
                    continue;
                foreach (var k in ImageVariants.AllKeys(key))
                    referenced.Add(k);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Orphan scan: không đọc được DB.");
            return new OrphanReport(0, 0, 0, 0, [], checkedAt, dryRun, Error: "DB_UNAVAILABLE");
        }

        // 2. Liệt kê MinIO, diff + lọc tuổi.
        var orphans = new List<(string Key, long Size)>();
        var total = 0;
        await foreach (var obj in storage.ListKeysAsync(effectivePrefix, ct))
        {
            total++;
            if (referenced.Contains(obj.Key))
                continue;
            var age = obj.LastModified.HasValue ? checkedAt - obj.LastModified.Value : TimeSpan.MaxValue;
            if (age < minAge)
                continue; // Presigned mới upload, chờ FE confirm.
            orphans.Add((obj.Key, obj.Size));
        }

        var orphanBytes = orphans.Sum(o => o.Size);
        var reportKeys = orphans.Select(o => o.Key).Take(maxKeysInReport).ToList();

        // 3. Xóa thật khi không dry-run.
        if (!dryRun)
        {
            foreach (var (key, _) in orphans)
            {
                try
                {
                    await storage.DeleteByKeyAsync(key, ct);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Orphan cleanup: xóa thất bại {Key}.", key);
                }
            }
            logger.LogInformation("Orphan cleanup: đã xóa {Count} file ({Bytes} bytes).", orphans.Count, orphanBytes);
        }
        else
        {
            logger.LogInformation("Orphan scan (dry-run): {Count}/{Total} file mồ côi.", orphans.Count, total);
        }

        return new OrphanReport(total, referenced.Count, orphans.Count, orphanBytes, reportKeys, checkedAt, dryRun);
    }

    private static async IAsyncEnumerable<string> ReferencedUrlsAsync(
        FoodBlogDbContext db,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var url in db.RecipeImages.AsNoTracking()
            .Select(i => i.OriginalUrl).AsAsyncEnumerable().WithCancellation(ct))
            if (!string.IsNullOrWhiteSpace(url)) yield return url;

        await foreach (var url in db.RecipeImages.AsNoTracking()
            .Where(i => i.MediumUrl != null).Select(i => i.MediumUrl!).AsAsyncEnumerable().WithCancellation(ct))
            if (!string.IsNullOrWhiteSpace(url)) yield return url;

        await foreach (var url in db.RecipeImages.AsNoTracking()
            .Where(i => i.ThumbnailUrl != null).Select(i => i.ThumbnailUrl!).AsAsyncEnumerable().WithCancellation(ct))
            if (!string.IsNullOrWhiteSpace(url)) yield return url;

        await foreach (var url in db.RecipeSteps.AsNoTracking()
            .Where(s => s.ImageUrl != null).Select(s => s.ImageUrl!).AsAsyncEnumerable().WithCancellation(ct))
            if (!string.IsNullOrWhiteSpace(url)) yield return url;
    }
}

/// <summary>Worker chạy quét orphan định kỳ (mặc định mỗi 24h).</summary>
public sealed class OrphanCleanupWorker(
    OrphanCleanupRunner runner,
    IOptions<OrphanCleanupOptions> options,
    ILogger<OrphanCleanupWorker> logger) : BackgroundService
{
    private readonly OrphanCleanupOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("OrphanCleanupWorker: disabled.");
            return;
        }
        using var timer = new PeriodicTimer(TimeSpan.FromHours(_options.IntervalHours));
        // Chạy 1 lần sau 2 phút khởi động để warm-up, sau đó theo chu kỳ.
        try
        {
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            await runner.ScanAsync(dryRun: false, ct: stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "OrphanCleanupWorker: lần quét đầu thất bại.");
        }
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
                await runner.ScanAsync(dryRun: false, ct: stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "OrphanCleanupWorker dừng bất thường.");
        }
    }
}
