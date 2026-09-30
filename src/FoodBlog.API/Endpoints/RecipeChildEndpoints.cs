using FoodBlog.Application.Features.Recipes;
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
public record ReorderIngredientRequest(Guid Id, int OrderIndex);

public static class RecipeChildEndpoints
{
    public static void MapRecipeChildEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/recipes/{id:guid}");

        group.MapPost("/steps", async (Guid id, CreateStepRequest req, FoodBlogDbContext db) =>
        {
            var recipe = await db.Recipes.FirstOrDefaultAsync(r => r.Id == id);
            if (recipe is null) return NotFound(id);
            if (string.IsNullOrWhiteSpace(req.Title))
                return Results.UnprocessableEntity(new { title = "Validation failed", errors = new { title = new[] { "Tiêu đề bước không được rỗng." } } });

            var number = await db.RecipeSteps.Where(s => s.RecipeId == id).CountAsync() + 1;
            var step = RecipeStep.Create(id, number,
                req.Title.Trim(), req.Description ?? string.Empty,
                req.TimerMinutes, req.ImageUrl);
            db.RecipeSteps.Add(step);
            await db.SaveChangesAsync();
            EvictListCache();
            return Results.Created($"/api/v1/recipes/{id}/steps/{step.Id}",
                new RecipeStepDto(step.Id, step.StepNumber, step.Title, step.Description, step.TimerMinutes, step.ImageUrl));
        });

        group.MapPut("/steps/{stepId:guid}", async (Guid id, Guid stepId, UpdateStepRequest req, FoodBlogDbContext db) =>
        {
            var step = await db.RecipeSteps.FirstOrDefaultAsync(s => s.Id == stepId && s.RecipeId == id);
            if (step is null) return Results.NotFound(new { title = "Không tìm thấy bước thực hiện.", stepId });
            step.Update(req.Title.Trim(), req.Description ?? string.Empty, req.TimerMinutes, req.ImageUrl);
            await db.SaveChangesAsync();
            EvictListCache();
            return Results.Ok(new RecipeStepDto(step.Id, step.StepNumber, step.Title, step.Description, step.TimerMinutes, step.ImageUrl));
        });

        group.MapDelete("/steps/{stepId:guid}", async (Guid id, Guid stepId, FoodBlogDbContext db) =>
        {
            var step = await db.RecipeSteps.FirstOrDefaultAsync(s => s.Id == stepId && s.RecipeId == id);
            if (step is null) return Results.NotFound(new { title = "Không tìm thấy bước thực hiện.", stepId });
            db.RecipeSteps.Remove(step);
            await db.SaveChangesAsync();
            var remaining = await db.RecipeSteps.Where(s => s.RecipeId == id).OrderBy(s => s.StepNumber).ToListAsync();
            for (var i = 0; i < remaining.Count; i++)
                remaining[i].SetStepNumber(i + 1);
            await db.SaveChangesAsync();
            EvictListCache();
            return Results.NoContent();
        });

        group.MapPost("/ingredients", async (Guid id, CreateIngredientRequest req, FoodBlogDbContext db) =>
        {
            var recipe = await db.Recipes.FirstOrDefaultAsync(r => r.Id == id);
            if (recipe is null) return NotFound(id);
            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.UnprocessableEntity(new { title = "Validation failed", errors = new { name = new[] { "Tên nguyên liệu không được rỗng." } } });

            var order = await db.RecipeIngredients.Where(i => i.RecipeId == id).CountAsync();
            var ingredient = RecipeIngredient.Create(id,
                req.Name.Trim(), req.Quantity, req.Unit, req.Notes, order);
            db.RecipeIngredients.Add(ingredient);
            await db.SaveChangesAsync();
            EvictListCache();
            return Results.Created($"/api/v1/recipes/{id}/ingredients/{ingredient.Id}",
                new RecipeIngredientDto(ingredient.Id, ingredient.Name, ingredient.Quantity, ingredient.Unit, ingredient.Notes, ingredient.OrderIndex));
        });

        group.MapPut("/ingredients/{ingId:guid}", async (Guid id, Guid ingId, UpdateIngredientRequest req, FoodBlogDbContext db) =>
        {
            var ingredient = await db.RecipeIngredients.FirstOrDefaultAsync(i => i.Id == ingId && i.RecipeId == id);
            if (ingredient is null) return Results.NotFound(new { title = "Không tìm thấy nguyên liệu.", ingId });
            ingredient.Update(req.Name.Trim(), req.Quantity, req.Unit, req.Notes);
            await db.SaveChangesAsync();
            EvictListCache();
            return Results.Ok(new RecipeIngredientDto(ingredient.Id, ingredient.Name, ingredient.Quantity, ingredient.Unit, ingredient.Notes, ingredient.OrderIndex));
        });

        group.MapDelete("/ingredients/{ingId:guid}", async (Guid id, Guid ingId, FoodBlogDbContext db) =>
        {
            var ingredient = await db.RecipeIngredients.FirstOrDefaultAsync(i => i.Id == ingId && i.RecipeId == id);
            if (ingredient is null) return Results.NotFound(new { title = "Không tìm thấy nguyên liệu.", ingId });
            db.RecipeIngredients.Remove(ingredient);
            await db.SaveChangesAsync();
            EvictListCache();
            return Results.NoContent();
        });

        group.MapPost("/images", async (Guid id, CreateImageRequest req, FoodBlogDbContext db) =>
        {
            var recipe = await db.Recipes.FirstOrDefaultAsync(r => r.Id == id);
            if (recipe is null) return NotFound(id);
            if (string.IsNullOrWhiteSpace(req.OriginalUrl))
                return Results.UnprocessableEntity(new { title = "Validation failed", errors = new { originalUrl = new[] { "URL ảnh không được rỗng." } } });

            var order = await db.RecipeImages.Where(i => i.RecipeId == id).CountAsync();
            var image = RecipeImage.Create(id,
                req.OriginalUrl.Trim(), null, null, req.AltText, order == 0, order);
            db.RecipeImages.Add(image);
            await db.SaveChangesAsync();
            EvictListCache();
            return Results.Created($"/api/v1/recipes/{id}/images/{image.Id}", ToImage(image));
        });

        group.MapPatch("/images/{imgId:guid}/primary", async (Guid id, Guid imgId, FoodBlogDbContext db) =>
        {
            var image = await db.RecipeImages.FirstOrDefaultAsync(i => i.Id == imgId && i.RecipeId == id);
            if (image is null) return Results.NotFound(new { title = "Không tìm thấy ảnh.", imgId });
            var siblings = await db.RecipeImages.Where(i => i.RecipeId == id).ToListAsync();
            foreach (var s in siblings)
                s.SetPrimary(s.Id == imgId);
            await db.SaveChangesAsync();
            EvictListCache();
            return Results.Ok(ToImage(image));
        });

        group.MapDelete("/images/{imgId:guid}", async (Guid id, Guid imgId, FoodBlogDbContext db) =>
        {
            var image = await db.RecipeImages.FirstOrDefaultAsync(i => i.Id == imgId && i.RecipeId == id);
            if (image is null) return Results.NotFound(new { title = "Không tìm thấy ảnh.", imgId });
            var wasPrimary = image.IsPrimary;
            db.RecipeImages.Remove(image);
            await db.SaveChangesAsync();
            if (wasPrimary)
            {
                var next = await db.RecipeImages.Where(i => i.RecipeId == id).OrderBy(i => i.OrderIndex).FirstOrDefaultAsync();
                if (next is not null)
                {
                    next.SetPrimary(true);
                    await db.SaveChangesAsync();
                }
            }
            EvictListCache();
            return Results.NoContent();
        });
    }

    private static IResult NotFound(Guid id) =>
        Results.NotFound(new { code = "RECIPE_NOT_FOUND", title = "Không tìm thấy công thức.", id });

    private static RecipeImageDto ToImage(RecipeImage i) => new(i.Id, i.OriginalUrl, i.MediumUrl, i.ThumbnailUrl,
        i.AltText, i.IsPrimary, i.OrderIndex,
        i.ThumbnailUrl is null && i.MediumUrl is null ? "pending" : "ready");
}
