namespace FoodBlog.Application.Options;

/// <summary>
/// TUẦN 4 (Trần Gia Huy) — Orphaned Files: cấu hình background cleanup.
/// </summary>
public sealed class OrphanCleanupOptions
{
    public const string SectionName = "OrphanCleanup";

    /// <summary>Bật/tắt worker quét đêm.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Chu kỳ quét (giờ). Mặc định 24h.</summary>
    public double IntervalHours { get; set; } = 24;

    /// <summary>Chỉ coi là orphan khi object cũ hơn N giờ (tha presigned mới upload chưa confirm).</summary>
    public double MinAgeHours { get; set; } = 24;

    public string Prefix { get; set; } = "recipes/";
}
