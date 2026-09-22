using FoodBlog.Application.Common.Models;
using FoodBlog.Application.DTOs;
using FoodBlog.Application.Features.Categories.Commands.CreateCategory;
using FoodBlog.Application.Features.Categories.Commands.DeleteCategory;
using FoodBlog.Application.Features.Categories.Commands.UpdateCategory;
using FoodBlog.Application.Features.Categories.Queries.GetCategories;
using FoodBlog.Application.Features.Categories.Queries.GetCategoryById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FoodBlog.API.Endpoints;

/// <summary>
/// RESTful endpoints cho Category (§1.3.1, §1.4.2).
/// URL Path Versioning: /api/v1/categories (§1.3.4).
/// </summary>
public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/categories")
            .WithTags("Categories");

        // GET /api/v1/categories?page=1&pageSize=10&search=&sortBy=name
        group.MapGet("/", async (
            [AsParameters] GetCategoriesQuery query,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(query, ct);
            return Results.Ok(result);
        })
        .WithName("GetCategories")
        .WithSummary("Lấy danh sách danh mục có phân trang")
        .WithDescription("Hỗ trợ search, sort, pagination. Ví dụ: ?search=monkho&page=2&pageSize=5")
        .Produces<PaginatedResult<CategoryDto>>(200);

        // GET /api/v1/categories/{id}
        group.MapGet("/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetCategoryByIdQuery(id), ct);
            return Results.Ok(result);
        })
        .WithName("GetCategoryById")
        .WithSummary("Lấy chi tiết danh mục theo Id")
        .Produces<CategoryDto>(200)
        .ProducesProblem(404);

        // POST /api/v1/categories
        group.MapPost("/", async (
            CreateCategoryCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/api/v1/categories/{result.Id}", result);
        })
        .WithName("CreateCategory")
        .WithSummary("Tạo danh mục mới")
        .Produces<CategoryDto>(201)
        .ProducesProblem(400)
        .ProducesProblem(409);

        // PUT /api/v1/categories/{id}
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateCategoryRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new UpdateCategoryCommand(id, request.Name, request.Description, request.ImageUrl, request.OrderIndex), ct);
            return Results.Ok(result);
        })
        .WithName("UpdateCategory")
        .WithSummary("Cập nhật danh mục (slug bất biến, FR-CAT-004)")
        .Produces<CategoryDto>(200)
        .ProducesProblem(404);

        // DELETE /api/v1/categories/{id}
        group.MapDelete("/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(new DeleteCategoryCommand(id), ct);
            return Results.NoContent();
        })
        .WithName("DeleteCategory")
        .WithSummary("Xóa danh mục (chặn khi còn recipe, FR-CAT-005)")
        .Produces(204)
        .ProducesProblem(404)
        .ProducesProblem(409);

        return app;
    }

    public record UpdateCategoryRequest(string Name, string? Description, string? ImageUrl = null, int? OrderIndex = null);
}
