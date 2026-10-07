using System.Net;
using System.Text.Json;
using FluentValidation;
using FoodBlog.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FoodBlog.API.Middlewares;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        var (status, code, title, errors) = MapException(ex);

        if (status == HttpStatusCode.InternalServerError)
            logger.LogError(ex, "Unhandled error {Method} {Path}", context.Request.Method, context.Request.Path);
        else
            logger.LogWarning(ex, "Handled error {Code} {Method} {Path}", code, context.Request.Method, context.Request.Path);

        var problem = new Dictionary<string, object?>
        {
            ["type"] = code is null
                ? "about:blank"
                : $"https://culinaryblog.local/problems/{code.ToLowerInvariant().Replace('_', '-')}",
            ["title"] = title,
            ["status"] = (int)status
        };
        if (code is not null)
            problem["code"] = code;
        if (errors is not null)
            problem["errors"] = errors;

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)status;
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }

    private static (HttpStatusCode Status, string? Code, string Title, Dictionary<string, string[]>? Errors)
        MapException(Exception ex) => ex switch
        {
            FluentValidation.ValidationException vex => (
                HttpStatusCode.UnprocessableEntity,
                "VALIDATION_ERROR",
                "Validation failed",
                vex.Errors
                    .GroupBy(f => ToCamelCase(f.PropertyName))
                    .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray())),

            RecipeNotFoundException rex => (
                HttpStatusCode.NotFound, rex.ErrorCode, rex.Message, null),

            RecipeForbiddenException rex => (
                HttpStatusCode.Forbidden, rex.ErrorCode, rex.Message, null),

            RecipeUnauthorizedException rex => (
                HttpStatusCode.Unauthorized, null, rex.Message, null),

            RecipeConcurrencyException rex => (
                HttpStatusCode.Conflict, rex.ErrorCode, rex.Message, null),

            RecipeSlugExistsException rex => (
                HttpStatusCode.Conflict, rex.ErrorCode, rex.Message, null),

            RecipePublishIncompleteException rex => (
                HttpStatusCode.UnprocessableEntity, rex.ErrorCode, rex.Message, null),

            DomainException dex => (
                HttpStatusCode.BadRequest, dex.ErrorCode, dex.Message, null),

            KeyNotFoundException => (
                HttpStatusCode.NotFound, "NOT_FOUND", "Không tìm thấy tài nguyên.", null),

            DbUpdateConcurrencyException => (
                HttpStatusCode.Conflict, "RECIPE_CONCURRENCY_CONFLICT",
                "Dữ liệu đã bị thay đổi bởi người khác.", null),

            _ => (
                HttpStatusCode.InternalServerError, null,
                "Đã xảy ra lỗi hệ thống.", null)
        };

    private static string ToCamelCase(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToLowerInvariant(s[0]) + s.Substring(1);
}
