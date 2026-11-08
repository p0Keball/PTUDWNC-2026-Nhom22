using System.Diagnostics.Metrics;

namespace FoodBlog.API;

/// <summary>
/// FR-OBS-003: custom metrics cho nghiệp vụ chính (công thức được tạo / xuất bản).
/// Đăng ký meter với OpenTelemetry trong Program.cs.
/// </summary>
public static class FoodBlogMetrics
{
    public const string MeterName = "FoodBlog.API";

    private static readonly Meter Meter = new(MeterName, "1.0.0");

    public static readonly Counter<long> RecipesCreated =
        Meter.CreateCounter<long>("foodblog.recipes.created", "recipes",
            "Số công thức được tạo mới.");

    public static readonly Counter<long> RecipesPublished =
        Meter.CreateCounter<long>("foodblog.recipes.published", "recipes",
            "Số công thức được xuất bản.");
}
