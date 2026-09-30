using System.Security.Claims;
using FluentValidation;
using FoodBlog.Application.Common;
using FoodBlog.Application.Interfaces;
using FoodBlog.Domain.Entities;
using FoodBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using static FoodBlog.API.Endpoints.RecipeEndpoints;

namespace FoodBlog.API.Endpoints;

public record CreateStepRequest(string Title, string Description, int? TimerMinutes, string? ImageUrl);
public record UpdateStepRequest(string Title, string Description, int? TimerMinutes, string? ImageUrl);
public record CreateIngredientRequest(string Name, decimal? Quantity, string? Unit, string? Notes);
public record UpdateIngredientRequest(string Name, decimal? Quantity, string? Unit, string? Notes);
public record CreateImageRequest(string OriginalUrl, string? AltText);
public record ReorderItemRequest(Guid Id, int Order);
public record PresignImageRequest(string FileName, string ContentType, long FileSize);
public record ConfirmImageRequest(string ObjectKey, string? AltText);

public static class RecipeChildEndpoints
{
    private static readonly CreateStepValidator CreateStepValidator = new();
    private static readonly UpdateStepValidator UpdateStepValidator = new();
    private static readonly CreateIngredientValidator CreateIngredientValidator = new();
    private static readonly UpdateIngredientValidator UpdateIngredientValidator = new();

    public static void MapRecipeChildEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/recipes/{id:guid}");

        group.MapPost("/steps", async (Guid id, CreateStepRequest req, FoodBlogDbContext db,
            ClaimsPrincipal user, CancellationToken ct) =>
        {
            var recipe = await db.Recipes.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (recipe is null) return NotFound(id);
            var forbidden = ForbidIfNotOwner(recipe.AuthorId, user);
            if (forbidden is not null) return forbidden;
            var invalid = ChildValidationResult.ToUnprocessable(CreateStepValidator, req);
            if (invalid is not null) return invalid;

            var number = await db.RecipeSteps.Where(s => s.RecipeId == id).CountAsync(ct) + 1;
            var step = RecipeStep.Create(id, number,
                req.Title.Trim(), req.Description ?? string.Empty,
                req.TimerMinutes, string.IsNullOrWhiteSpace(req.ImageUrl) ? null : req.ImageUrl.Trim());
            db.RecipeSteps.Add(step);
            await db.SaveChangesAsync(ct);
            EvictListCache();
            return Results.Created($"/api/v1/recipes/{id}/steps/{step.Id}",
                new StepDto(step.Id, step.StepNumber, step.Title, step.Description, step.TimerMinutes, step.ImageUrl));
        });

        group.MapPut("/steps/{stepId:guid}", async (Guid id, Guid stepId, UpdateStepRequest req,
            FoodBlogDbContext db, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var forbidden = await CheckOwnerAsync(db, id, user, ct);
            if (forbidden is not null) return forbidden;
            var step = await db.RecipeSteps.FirstOrDefaultAsync(s => s.Id == stepId && s.RecipeId == id, ct);
            if (step is null) return Results.NotFound(new { title = "Không tìm thấy bước thực hiện.", stepId });
            var invalid = ChildValidationResult.ToUnprocessable(UpdateStepValidator, req);
            if (invalid is not null) return invalid;
            step.Update(req.Title.Trim(), req.Description ?? string.Empty,
                req.TimerMinutes, string.IsNullOrWhiteSpace(req.ImageUrl) ? null : req.ImageUrl.Trim());
            await db.SaveChangesAsync(ct);
            EvictListCache();
            return Results.Ok(new StepDto(step.Id, step.StepNumber, step.Title, step.Description, step.TimerMinutes, step.ImageUrl));
        });

        group.MapDelete("/steps/{stepId:guid}", async (Guid id, Guid stepId, FoodBlogDbContext db,
            ClaimsPrincipal user, CancellationToken ct) =>
        {
            var forbidden = await CheckOwnerAsync(db, id, user, ct);
            if (forbidden is not null) return forbidden;
            var step = await db.RecipeSteps.FirstOrDefaultAsync(s => s.Id == stepId && s.RecipeId == id, ct);
            if (step is null) return Results.NotFound(new { title = "Không tìm thấy bước thực hiện.", stepId });
            db.RecipeSteps.Remove(step);
            await db.SaveChangesAsync(ct);
            await RenumberStepsAsync(db, id);
            EvictListCache();
            return Results.NoContent();
        });

        // Drag & Drop: FE gửi toàn bộ thứ tự mới sau khi kéo thả.
        group.MapPatch("/steps/reorder", async (Guid id, List<ReorderItemRequest> req, FoodBlogDbContext db,
            ClaimsPrincipal user, CancellationToken ct) =>
        {
            var recipe = await db.Recipes.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (recipe is null) return NotFound(id);
            var forbidden = ForbidIfNotOwner(recipe.AuthorId, user);
            if (forbidden is not null) return forbidden;
            if (req.Count == 0)
                return Results.UnprocessableEntity(new { title = "Danh sách reorder rỗng." });

            var steps = await db.RecipeSteps.Where(s => s.RecipeId == id).ToListAsync(ct);
            var byId = steps.ToDictionary(s => s.Id);
            if (req.Count != steps.Count || req.Any(r => !byId.ContainsKey(r.Id)))
                return Results.UnprocessableEntity(new { title = "Danh sách reorder không khớp với số bước hiện tại." });

            var orderedIds = req.OrderBy(r => r.Order).Select(r => r.Id).ToList();
            for (var i = 0; i < orderedIds.Count; i++)
                byId[orderedIds[i]].Renumber(i + 1);
            await db.SaveChangesAsync(ct);
            EvictListCache();
            var result = await db.RecipeSteps.Where(s => s.RecipeId == id).OrderBy(s => s.StepNumber)
                .Select(s => new StepDto(s.Id, s.StepNumber, s.Title, s.Description, s.TimerMinutes, s.ImageUrl))
                .ToListAsync(ct);
            return Results.Ok(result);
        });

        group.MapPost("/ingredients", async (Guid id, CreateIngredientRequest req, FoodBlogDbContext db,
            ClaimsPrincipal user, CancellationToken ct) =>
        {
            var recipe = await db.Recipes.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (recipe is null) return NotFound(id);
            var forbidden = ForbidIfNotOwner(recipe.AuthorId, user);
            if (forbidden is not null) return forbidden;
            var invalid = ChildValidationResult.ToUnprocessable(CreateIngredientValidator, req);
            if (invalid is not null) return invalid;

            var order = await db.RecipeIngredients.Where(i => i.RecipeId == id).CountAsync(ct);
            var ingredient = RecipeIngredient.Create(id, req.Name.Trim(), req.Quantity,
                string.IsNullOrWhiteSpace(req.Unit) ? null : req.Unit.Trim(),
                string.IsNullOrWhiteSpace(req.Notes) ? null : req.Notes.Trim(), order);
            db.RecipeIngredients.Add(ingredient);
            await db.SaveChangesAsync(ct);
            EvictListCache();
            return Results.Created($"/api/v1/recipes/{id}/ingredients/{ingredient.Id}",
                new IngredientDto(ingredient.Id, ingredient.Name, ingredient.Quantity, ingredient.Unit, ingredient.Notes, ingredient.OrderIndex));
        });

        group.MapPut("/ingredients/{ingId:guid}", async (Guid id, Guid ingId, UpdateIngredientRequest req,
            FoodBlogDbContext db, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var forbidden = await CheckOwnerAsync(db, id, user, ct);
            if (forbidden is not null) return forbidden;
            var ingredient = await db.RecipeIngredients.FirstOrDefaultAsync(i => i.Id == ingId && i.RecipeId == id, ct);
            if (ingredient is null) return Results.NotFound(new { title = "Không tìm thấy nguyên liệu.", ingId });
            var invalid = ChildValidationResult.ToUnprocessable(UpdateIngredientValidator, req);
            if (invalid is not null) return invalid;
            ingredient.Update(req.Name.Trim(), req.Quantity,
                string.IsNullOrWhiteSpace(req.Unit) ? null : req.Unit.Trim(),
                string.IsNullOrWhiteSpace(req.Notes) ? null : req.Notes.Trim());
            await db.SaveChangesAsync(ct);
            EvictListCache();
            return Results.Ok(new IngredientDto(ingredient.Id, ingredient.Name, ingredient.Quantity, ingredient.Unit, ingredient.Notes, ingredient.OrderIndex));
        });

        group.MapDelete("/ingredients/{ingId:guid}", async (Guid id, Guid ingId, FoodBlogDbContext db,
            ClaimsPrincipal user, CancellationToken ct) =>
        {
            var forbidden = await CheckOwnerAsync(db, id, user, ct);
            if (forbidden is not null) return forbidden;
            var ingredient = await db.RecipeIngredients.FirstOrDefaultAsync(i => i.Id == ingId && i.RecipeId == id, ct);
            if (ingredient is null) return Results.NotFound(new { title = "Không tìm thấy nguyên liệu.", ingId });
            db.RecipeIngredients.Remove(ingredient);
            await db.SaveChangesAsync(ct);
            await RenumberIngredientsAsync(db, id);
            EvictListCache();
            return Results.NoContent();
        });

        group.MapPatch("/ingredients/reorder", async (Guid id, List<ReorderItemRequest> req,
            FoodBlogDbContext db, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var recipe = await db.Recipes.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (recipe is null) return NotFound(id);
            var forbidden = ForbidIfNotOwner(recipe.AuthorId, user);
            if (forbidden is not null) return forbidden;
            if (req.Count == 0)
                return Results.UnprocessableEntity(new { title = "Danh sách reorder rỗng." });

            var ingredients = await db.RecipeIngredients.Where(i => i.RecipeId == id).ToListAsync(ct);
            var byId = ingredients.ToDictionary(i => i.Id);
            if (req.Count != ingredients.Count || req.Any(r => !byId.ContainsKey(r.Id)))
                return Results.UnprocessableEntity(new { title = "Danh sách reorder không khớp với số nguyên liệu hiện tại." });

            var orderedIds = req.OrderBy(r => r.Order).Select(r => r.Id).ToList();
            for (var i = 0; i < orderedIds.Count; i++)
                byId[orderedIds[i]].SetOrder(i);
            await db.SaveChangesAsync(ct);
            EvictListCache();
            var result = await db.RecipeIngredients.Where(i => i.RecipeId == id).OrderBy(i => i.OrderIndex)
                .Select(i => new IngredientDto(i.Id, i.Name, i.Quantity, i.Unit, i.Notes, i.OrderIndex))
                .ToListAsync(ct);
            return Results.Ok(result);
        });

        // FR-RCP-008 / §8.4: upload multipart/form-data đúng đặc tả SRS.
        group.MapPost("/images", async (Guid id, IFormFile file, FoodBlogDbContext db,
            IFileStorageService storage, ClaimsPrincipal user, CancellationToken ct,
            string? altText = null) =>
        {
            var recipe = await db.Recipes.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (recipe is null) return NotFound(id);
            var forbidden = ForbidIfNotOwner(recipe.AuthorId, user);
            if (forbidden is not null) return forbidden;
            if (file is null || file.Length == 0)
                return FileError("FILE_MIME_INVALID", "File ảnh rỗng.",
                    new { file = new[] { "Vui lòng chọn file ảnh." } });
            var extError = ValidateUploadMeta(file.FileName, file.ContentType, file.Length);
            if (extError is not null) return extError;

            byte[] leading;
            try
            {
                leading = await ReadLeadingBytesAsync(file, 12, ct);
            }
            catch (Exception)
            {
                return Results.Json(new { code = "STORAGE_UNAVAILABLE", title = "Không đọc được file upload." }, statusCode: 503);
            }
            if (!ImageValidation.HasValidMagicBytes(leading, file.ContentType))
                return FileError("FILE_MIME_INVALID", "Định dạng tệp không được hỗ trợ (chỉ nhận JPG, PNG, WebP, AVIF).",
                    new { file = new[] { "File không phải ảnh hợp lệ (magic bytes không khớp MIME)." } });

            string publicUrl;
            try
            {
                publicUrl = await storage.UploadAsync(file, $"recipes/{id}", ct);
            }
            catch (Exception)
            {
                return Results.Json(new { code = "STORAGE_UNAVAILABLE", title = "Không lưu được ảnh lên MinIO, vui lòng thử lại." }, statusCode: 503);
            }

            var order = await db.RecipeImages.Where(i => i.RecipeId == id).CountAsync(ct);
            var image = RecipeImage.Create(id, publicUrl, null, null,
                string.IsNullOrWhiteSpace(altText) ? null : altText.Trim(),
                order == 0, order);
            db.RecipeImages.Add(image);
            await db.SaveChangesAsync(ct);
            EvictListCache();
            return Results.Created($"/api/v1/recipes/{id}/images/{image.Id}", ToImage(image));
        }).DisableAntiforgery();

        // Biến thể tuần 1: tạo bản ghi ảnh từ URL có sẵn.
        group.MapPost("/images/by-url", async (Guid id, CreateImageRequest req, FoodBlogDbContext db,
            ClaimsPrincipal user, CancellationToken ct) =>
        {
            var recipe = await db.Recipes.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (recipe is null) return NotFound(id);
            var forbidden = ForbidIfNotOwner(recipe.AuthorId, user);
            if (forbidden is not null) return forbidden;
            if (string.IsNullOrWhiteSpace(req.OriginalUrl))
                return Results.UnprocessableEntity(new { title = "Validation failed", errors = new { originalUrl = new[] { "URL ảnh không được rỗng." } } });

            var order = await db.RecipeImages.Where(i => i.RecipeId == id).CountAsync(ct);
            var image = RecipeImage.Create(id, req.OriginalUrl.Trim(), null, null,
                string.IsNullOrWhiteSpace(req.AltText) ? null : req.AltText.Trim(),
                order == 0, order);
            db.RecipeImages.Add(image);
            await db.SaveChangesAsync(ct);
            EvictListCache();
            return Results.Created($"/api/v1/recipes/{id}/images/{image.Id}", ToImage(image));
        });

        // Bước 1 presigned-URL: validate MIME + extension + 5MB trước khi cấp URL.
        group.MapPost("/images/presign", async (Guid id, PresignImageRequest req, FoodBlogDbContext db,
            IFileStorageService storage, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var recipe = await db.Recipes.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (recipe is null) return NotFound(id);
            var forbidden = ForbidIfNotOwner(recipe.AuthorId, user);
            if (forbidden is not null) return forbidden;

            var metaError = ValidateUploadMeta(req.FileName, req.ContentType, req.FileSize);
            if (metaError is not null) return metaError;

            var objectKey = ImageValidation.BuildObjectKey(id, req.FileName);
            var presigned = await storage.GetPresignedPutUrlAsync(objectKey, req.ContentType, TimeSpan.FromMinutes(5), ct);
            return Results.Ok(new { uploadUrl = presigned.UploadUrl, objectKey = presigned.ObjectKey, publicUrl = presigned.PublicUrl, expiresAt = presigned.ExpiresAt });
        });

        // Bước 2 confirm: kiểm tra object thực tế trên MinIO (size + magic bytes) rồi mới lưu DB.
        group.MapPost("/images/confirm", async (Guid id, ConfirmImageRequest req, FoodBlogDbContext db,
            IFileStorageService storage, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var recipe = await db.Recipes.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (recipe is null) return NotFound(id);
            var forbidden = ForbidIfNotOwner(recipe.AuthorId, user);
            if (forbidden is not null) return forbidden;
            if (string.IsNullOrWhiteSpace(req.ObjectKey) || req.ObjectKey.Contains(".."))
                return Results.UnprocessableEntity(new { title = "Object key không hợp lệ." });

            StoredFileMetadata? meta;
            try
            {
                meta = await storage.GetMetadataAsync(req.ObjectKey, ct);
            }
            catch (Exception)
            {
                return Results.Json(new { code = "STORAGE_UNAVAILABLE", title = "Không kết nối được MinIO, vui lòng thử lại." }, statusCode: 503);
            }

            if (meta is null)
                return Results.UnprocessableEntity(new { title = "File chưa được upload lên MinIO." });
            if (meta.Size > ImageValidation.MaxFileSizeBytes)
            {
                await storage.DeleteByKeyAsync(req.ObjectKey, ct);
                return FileError("FILE_SIZE_EXCEEDED", "Dung lượng tệp tải lên vượt quá hạn mức 5MB.",
                    new { fileSize = new[] { "File tối đa 5MB." } });
            }
            if (!ImageValidation.IsContentTypeAllowed(meta.ContentType))
            {
                await storage.DeleteByKeyAsync(req.ObjectKey, ct);
                return FileError("FILE_MIME_INVALID", "Định dạng tệp không được hỗ trợ (chỉ nhận JPG, PNG, WebP, AVIF).",
                    new { contentType = new[] { "MIME type không hỗ trợ." } });
            }

            byte[] leading;
            try
            {
                leading = await storage.ReadLeadingBytesAsync(req.ObjectKey, 12, ct);
            }
            catch (Exception)
            {
                return Results.Json(new { code = "STORAGE_UNAVAILABLE", title = "Không đọc được file trên MinIO." }, statusCode: 503);
            }

            if (!ImageValidation.HasValidMagicBytes(leading, meta.ContentType))
            {
                await storage.DeleteByKeyAsync(req.ObjectKey, ct);
                return FileError("FILE_MIME_INVALID", "Định dạng tệp không được hỗ trợ (chỉ nhận JPG, PNG, WebP, AVIF).",
                    new { file = new[] { "File không phải ảnh hợp lệ (magic bytes không khớp MIME)." } });
            }

            var order = await db.RecipeImages.Where(i => i.RecipeId == id).CountAsync(ct);
            var image = RecipeImage.Create(id, storage.GetPublicUrl(req.ObjectKey), null, null,
                string.IsNullOrWhiteSpace(req.AltText) ? null : req.AltText.Trim(),
                order == 0, order);
            db.RecipeImages.Add(image);
            await db.SaveChangesAsync(ct);
            EvictListCache();
            return Results.Created($"/api/v1/recipes/{id}/images/{image.Id}", ToImage(image));
        });

        group.MapPatch("/images/{imgId:guid}/primary", async (Guid id, Guid imgId, FoodBlogDbContext db,
            ClaimsPrincipal user, CancellationToken ct) =>
        {
            var forbidden = await CheckOwnerAsync(db, id, user, ct);
            if (forbidden is not null) return forbidden;
            var image = await db.RecipeImages.FirstOrDefaultAsync(i => i.Id == imgId && i.RecipeId == id, ct);
            if (image is null) return Results.NotFound(new { title = "Không tìm thấy ảnh.", imgId });
            var siblings = await db.RecipeImages.Where(i => i.RecipeId == id).ToListAsync(ct);
            foreach (var s in siblings)
                s.SetPrimary(s.Id == imgId);
            await db.SaveChangesAsync(ct);
            EvictListCache();
            return Results.Ok(ToImage(image));
        });

        group.MapDelete("/images/{imgId:guid}", async (Guid id, Guid imgId, FoodBlogDbContext db,
            IFileStorageService storage, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var forbidden = await CheckOwnerAsync(db, id, user, ct);
            if (forbidden is not null) return forbidden;
            var image = await db.RecipeImages.FirstOrDefaultAsync(i => i.Id == imgId && i.RecipeId == id, ct);
            if (image is null) return Results.NotFound(new { title = "Không tìm thấy ảnh.", imgId });
            var wasPrimary = image.IsPrimary;
            var originalUrl = image.OriginalUrl;
            db.RecipeImages.Remove(image);
            await db.SaveChangesAsync(ct);
            // Xóa object trên MinIO (best-effort + retry trong storage, không fail request nếu MinIO lỗi).
            try { await storage.DeleteAsync(originalUrl, ct); } catch { /* graceful fallback */ }
            if (wasPrimary)
            {
                var next = await db.RecipeImages.Where(i => i.RecipeId == id).OrderBy(i => i.OrderIndex).FirstOrDefaultAsync(ct);
                if (next is not null)
                {
                    next.SetPrimary(true);
                    await db.SaveChangesAsync(ct);
                }
            }
            await RenumberImagesAsync(db, id);
            EvictListCache();
            return Results.NoContent();
        });
    }

    private static IResult NotFound(Guid id) =>
        Results.NotFound(new { code = "RECIPE_NOT_FOUND", title = "Không tìm thấy công thức.", id });

    private static IResult FileError(string code, string title, object errors) =>
        Results.Json(new { code, title, errors }, statusCode: 400);

    private static IResult? ValidateUploadMeta(string fileName, string contentType, long fileSize)
    {
        if (!ImageValidation.IsContentTypeAllowed(contentType))
            return FileError("FILE_MIME_INVALID", "Định dạng tệp không được hỗ trợ (chỉ nhận JPG, PNG, WebP, AVIF).",
                new { contentType = new[] { "MIME type không hỗ trợ. Chỉ chấp nhận image/jpeg, image/png, image/webp, image/avif." } });
        if (!ImageValidation.IsExtensionAllowed(fileName))
            return FileError("FILE_MIME_INVALID", "Định dạng tệp không được hỗ trợ (chỉ nhận JPG, PNG, WebP, AVIF).",
                new { fileName = new[] { "Đuôi file không hỗ trợ. Chỉ chấp nhận .jpg, .jpeg, .png, .webp, .avif." } });
        if (fileSize <= 0 || fileSize > ImageValidation.MaxFileSizeBytes)
            return FileError("FILE_SIZE_EXCEEDED", "Dung lượng tệp tải lên vượt quá hạn mức 5MB.",
                new { fileSize = new[] { "File tối đa 5MB." } });
        return null;
    }

    private static async Task<byte[]> ReadLeadingBytesAsync(IFormFile file, int count, CancellationToken ct)
    {
        using var stream = file.OpenReadStream();
        var buffer = new byte[count];
        var read = 0;
        while (read < count)
        {
            var n = await stream.ReadAsync(buffer.AsMemory(read, count - read), ct);
            if (n == 0) break;
            read += n;
        }
        return read == count ? buffer : buffer[..read];
    }

    /// <summary>
    /// Resource-Based Authorization (NFR-SEC-006): Author chỉ thao tác recipe của mình,
    /// Admin bypass. Khi request ẩn danh (JWT của Long chưa đấu nối) tạm cho qua để
    /// tương thích, khi có JWT sẽ enforce và trả 403 RECIPE_FORBIDDEN.
    /// </summary>
    private static IResult? ForbidIfNotOwner(string authorId, ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
            return null;
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != authorId && !user.IsInRole("Admin"))
            return Results.Json(new { code = "RECIPE_FORBIDDEN", title = "Bạn không có quyền thao tác trên công thức này." }, statusCode: 403);
        return null;
    }

    private static async Task<IResult?> CheckOwnerAsync(
        FoodBlogDbContext db, Guid recipeId, ClaimsPrincipal user, CancellationToken ct)
    {
        if (user.Identity?.IsAuthenticated != true)
            return null;
        var authorId = await db.Recipes.Where(r => r.Id == recipeId)
            .Select(r => r.AuthorId).FirstOrDefaultAsync(ct);
        if (authorId is null)
            return null; // Recipe không tồn tại — handler sẽ trả 404.
        return ForbidIfNotOwner(authorId, user);
    }

    private static ImageDto ToImage(RecipeImage i) => new(i.Id, i.OriginalUrl, i.MediumUrl, i.ThumbnailUrl,
        i.AltText, i.IsPrimary, i.OrderIndex,
        i.ThumbnailUrl is null && i.MediumUrl is null ? "pending" : "ready");

    private static async Task RenumberStepsAsync(FoodBlogDbContext db, Guid recipeId)
    {
        var remaining = await db.RecipeSteps.Where(s => s.RecipeId == recipeId).OrderBy(s => s.StepNumber).ToListAsync();
        for (var i = 0; i < remaining.Count; i++)
            remaining[i].Renumber(i + 1);
        await db.SaveChangesAsync();
    }

    private static async Task RenumberIngredientsAsync(FoodBlogDbContext db, Guid recipeId)
    {
        var remaining = await db.RecipeIngredients.Where(i => i.RecipeId == recipeId).OrderBy(i => i.OrderIndex).ToListAsync();
        for (var i = 0; i < remaining.Count; i++)
            remaining[i].SetOrder(i);
        await db.SaveChangesAsync();
    }

    private static async Task RenumberImagesAsync(FoodBlogDbContext db, Guid recipeId)
    {
        var remaining = await db.RecipeImages.Where(i => i.RecipeId == recipeId).OrderBy(i => i.OrderIndex).ToListAsync();
        for (var i = 0; i < remaining.Count; i++)
            remaining[i].SetOrder(i);
        await db.SaveChangesAsync();
    }
}
