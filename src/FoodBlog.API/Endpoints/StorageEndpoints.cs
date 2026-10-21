using FoodBlog.Application.Interfaces;
using FoodBlog.Infrastructure.Persistence;
using FoodBlog.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.API.Endpoints;

/// <summary>
/// TUẦN 4 (Trần Gia Huy) — MinIO Reliability + Orphaned Files + Form Validation checklist.
/// </summary>
public static class StorageEndpoints
{
    public static void MapStorageEndpoints(this WebApplication app)
    {
        // Graceful fallback probe cho FE: poll để hiện banner khi storage disconnect.
        app.MapGet("/api/v1/storage/health", async (IFileStorageService storage, CancellationToken ct) =>
        {
            var healthy = await storage.IsHealthyAsync(ct);
            var body = new { status = healthy ? "healthy" : "unhealthy", checkedAt = DateTimeOffset.UtcNow };
            return healthy ? Results.Ok(body) : Results.Json(body, statusCode: 503);
        });

        // Orphan dry-run: xem trước file mồ côi, không xóa.
        app.MapGet("/api/v1/admin/storage/orphans", async (
            OrphanCleanupRunner runner, string? prefix, double? minAgeHours, CancellationToken ct) =>
        {
            var report = await runner.ScanAsync(dryRun: true, prefix: prefix, minAgeHours: minAgeHours, ct: ct);
            if (report.Error is not null)
                return Results.Json(new { code = "STORAGE_UNAVAILABLE", title = "Không quét được storage." }, statusCode: 503);
            return Results.Ok(report);
        });

        // Orphan cleanup thật.
        app.MapPost("/api/v1/admin/storage/orphans/cleanup", async (
            OrphanCleanupRunner runner, string? prefix, double? minAgeHours, CancellationToken ct) =>
        {
            var report = await runner.ScanAsync(dryRun: false, prefix: prefix, minAgeHours: minAgeHours, ct: ct);
            if (report.Error is not null)
                return Results.Json(new { code = "STORAGE_UNAVAILABLE", title = "Không quét được storage." }, statusCode: 503);
            return Results.Ok(report);
        });

        // Form Validation checklist: FE hiện tick trước khi cho Publish.
        app.MapGet("/api/v1/recipes/{id:guid}/validation", async (Guid id, FoodBlogDbContext db, CancellationToken ct) =>
        {
            var exists = await db.Recipes.AnyAsync(r => r.Id == id, ct);
            if (!exists)
                return Results.NotFound(new { code = "RECIPE_NOT_FOUND", title = "Không tìm thấy công thức.", id });
            var steps = await db.RecipeSteps.CountAsync(s => s.RecipeId == id, ct);
            var ingredients = await db.RecipeIngredients.CountAsync(i => i.RecipeId == id, ct);
            var images = await db.RecipeImages.CountAsync(i => i.RecipeId == id, ct);
            var issues = new List<string>();
            if (steps == 0) issues.Add("Cần ít nhất 1 bước thực hiện.");
            if (ingredients == 0) issues.Add("Cần ít nhất 1 nguyên liệu.");
            if (images == 0) issues.Add("Nên có ít nhất 1 ảnh bìa.");
            return Results.Ok(new
            {
                recipeId = id,
                hasSteps = steps > 0,
                hasIngredients = ingredients > 0,
                hasCoverImage = images > 0,
                stepsCount = steps,
                ingredientsCount = ingredients,
                imagesCount = images,
                canPublish = steps > 0, // BE publish hiện chỉ bắt buộc Steps > 0
                issues,
            });
        });
    }
}
