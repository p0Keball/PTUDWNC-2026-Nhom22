using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs;
using CulinaryBlog.Application.Features.Recipes.Commands.CreateRecipe;
using CulinaryBlog.Application.Features.Recipes.Queries.GetRecipeById;
using CulinaryBlog.Application.Features.Recipes.Queries.GetRecipes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CulinaryBlog.API.Endpoints;

public static class RecipeEndpoints
{
    public static IEndpointRouteBuilder MapRecipeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/recipes")
                       .WithTags("Recipes");

        // 1. GET /api/v1/recipes (Thêm [FromServices] cho sender)
        group.MapGet("/", async (
            [AsParameters] GetRecipesQuery query, 
            [FromServices] ISender sender, 
            CancellationToken ct) =>
        {
            var result = await sender.Send(query, ct);
            return Results.Ok(result);
        })
        .WithName("GetRecipes")
        .WithSummary("Lấy danh sách công thức nấu ăn")
        .WithDescription("Hỗ trợ lọc theo categoryId, difficulty, minCookTime, maxCookTime và phân trang.")
        .Produces<PaginatedResult<RecipeDto>>(200);

        // 2. GET /api/v1/recipes/{id} (Thêm [FromServices] cho sender)
        group.MapGet("/{id:guid}", async (
            Guid id, 
            [FromServices] ISender sender, 
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetRecipeByIdQuery(id), ct);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetRecipeById")
        .WithSummary("Lấy chi tiết công thức nấu ăn")
        .Produces<RecipeDetailDto>(200)
        .Produces(404);

        // 3. POST /api/v1/recipes (Thêm [FromServices] cho sender)
        group.MapPost("/", async (
            CreateRecipeCommand command, 
            [FromServices] ISender sender, 
            CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return Results.Created($"/api/v1/recipes/{result.Id}", result);
        })
        .WithName("CreateRecipe")
        .WithSummary("Tạo công thức mới")
        .Produces<RecipeDto>(201)
        .ProducesProblem(400);

        // 4. GET /api/v1/categories/{categoryId}/recipes (Thêm [FromServices] cho sender)
        app.MapGroup("/api/v1/categories")
           .WithTags("Categories")
           .MapGet("/{categoryId:guid}/recipes", async (
               Guid categoryId, 
               [FromQuery] int page, 
               [FromQuery] int pageSize, 
               [FromServices] ISender sender, 
               CancellationToken ct) =>
           {
               var query = new GetRecipesQuery(
                   CategoryId: categoryId, 
                   Page: page <= 0 ? 1 : page, 
                   PageSize: pageSize <= 0 ? 10 : pageSize);
                   
               var result = await sender.Send(query, ct);
               return Results.Ok(result);
           })
           .WithName("GetRecipesByCategory")
           .WithSummary("Lấy danh sách các bài viết thuộc một Category")
           .Produces<PaginatedResult<RecipeDto>>(200);

        return app;
    }
}