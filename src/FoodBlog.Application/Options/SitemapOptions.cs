namespace FoodBlog.Application.Options;

/// <summary>
/// FR-SEO-001: cấu hình job làm mới sitemap và ping search engine.
/// </summary>
public sealed class SitemapOptions
{
    public const string SectionName = "Sitemap";

    /// <summary>URL công khai của website (không có dấu / ở cuối).</summary>
    public string BaseUrl { get; set; } = "http://localhost:3000";

    /// <summary>
    /// URL dùng để gọi sitemap từ trong mạng nội bộ (ví dụ http://nginx/sitemap.xml).
    /// Để trống để dùng BaseUrl.
    /// </summary>
    public string FetchUrl { get; set; } = string.Empty;

    /// <summary>
    /// Endpoint ping sitemap (ví dụ https://www.google.com/ping).
    /// Để trống để tắt ping.
    /// </summary>
    public string SearchEnginePingUrl { get; set; } = string.Empty;
}
