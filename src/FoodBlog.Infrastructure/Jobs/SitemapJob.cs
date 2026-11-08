using FoodBlog.Application.Options;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodBlog.Infrastructure.Jobs;

/// <summary>
/// FR-SEO-001: sitemap.xml đã được Next.js sinh động từ dữ liệu API.
/// Job này chạy định kỳ để làm mới cache sitemap và ping search engine,
/// tránh sinh trùng một sitemap thứ hai ở backend.
/// </summary>
public sealed class SitemapJob(
    IOptions<SitemapOptions> options,
    HttpClient httpClient,
    ILogger<SitemapJob> logger)
{
    [AutomaticRetry(
        Attempts = 3,
        DelaysInSeconds = new[] { 60, 300, 1800 },
        OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task GenerateAsync(CancellationToken ct = default)
    {
        var opts = options.Value;
        var baseUrl = (opts.BaseUrl ?? string.Empty).Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("Sitemap:BaseUrl chưa được cấu hình.");

        var publicSitemapUrl = $"{baseUrl}/sitemap.xml";
        var fetchUrl = string.IsNullOrWhiteSpace(opts.FetchUrl)
            ? publicSitemapUrl
            : opts.FetchUrl.Trim();

        // Gọi sitemap để làm mới cache của Next.js và phát hiện lỗi sớm.
        try
        {
            using var response = await httpClient.GetAsync(fetchUrl, ct);
            if (response.IsSuccessStatusCode)
                logger.LogInformation("Sitemap: đã làm mới {Url}.", fetchUrl);
            else
                logger.LogWarning("Sitemap: {Url} trả về {StatusCode}.", fetchUrl, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Sitemap: không gọi được {Url}.", fetchUrl);
        }

        await PingSearchEngineAsync(publicSitemapUrl, opts, ct);
    }

    private async Task PingSearchEngineAsync(string sitemapUrl, SitemapOptions opts, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(opts.SearchEnginePingUrl))
            return;

        var pingBase = opts.SearchEnginePingUrl.Trim();
        var separator = pingBase.Contains('?') ? '&' : '?';
        var target = $"{pingBase}{separator}sitemap={Uri.EscapeDataString(sitemapUrl)}";

        try
        {
            using var response = await httpClient.GetAsync(target, ct);
            if (response.IsSuccessStatusCode)
                logger.LogInformation("Sitemap: ping thành công tới {Url}.", target);
            else
                logger.LogWarning("Sitemap: ping tới {Url} trả về {StatusCode}.", target, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            // Ping là best-effort, không làm job thất bại.
            logger.LogWarning(ex, "Sitemap: không ping được tới {Url}.", target);
        }
    }
}
