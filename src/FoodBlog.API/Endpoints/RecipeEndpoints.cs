using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using FoodBlog.Application.Interfaces;

namespace FoodBlog.API.Endpoints;

public static class RecipeEndpoints
{
    public static void MapRecipeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/recipes").RequireAuthorization();

        group.MapPost("/{id:guid}/images", async (Guid id, IFormFile file, IFileStorageService storageService, CancellationToken ct) =>
        {
            // Validation kích thước và định dạng[cite: 1]
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/avif" };
            if (!allowedMimeTypes.Contains(file.ContentType))
                return Results.BadRequest(new { error = "Chỉ chấp nhận JPEG, PNG, WebP, AVIF." });

            if (file.Length > 5 * 1024 * 1024)
                return Results.BadRequest(new { error = "Kích thước file vượt quá 5MB." });

            // (Lưu ý: Bạn nên có logic đọc Magic Bytes ở đây để an toàn tuyệt đối)

            var fileUrl = await storageService.UploadAsync(file, $"recipes/{id}", ct);
            
            // Dispatch Command lưu DB qua MediatR tại đây...
            // var imageId = await mediator.Send(new AddRecipeImageCommand(id, fileUrl));

            return Results.Created($"/api/v1/recipes/{id}/images", new { url = fileUrl, isPrimary = true });
        })
        .DisableAntiforgery(); // Tắt antiforgery nếu dùng client rời (Next.js)
    }
}