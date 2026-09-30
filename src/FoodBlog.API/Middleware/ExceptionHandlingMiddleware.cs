using System.Net;
using System.Text.Json;
using FoodBlog.Domain.Exceptions;
using FluentValidation;

namespace FoodBlog.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, IHostEnvironment env)
{
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

    private Task HandleAsync(HttpContext context, Exception ex)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        string? code = null;
        string title = "Đã xảy ra lỗi hệ thống.";
        IDictionary<string, string[]>? errors = null;
        string? detail = null;

        switch (ex)
        {
            case Domain.Exceptions.ValidationException vex:
                statusCode = (HttpStatusCode)vex.StatusCode;
                code = vex.Code;
                title = vex.Message;
                errors = vex.Errors;
                break;
            case DomainException dex:
                statusCode = (HttpStatusCode)dex.StatusCode;
                code = dex.Code;
                title = dex.Message;
                break;
            case FluentValidation.ValidationException fvex:
                statusCode = HttpStatusCode.UnprocessableEntity;
                code = "VALIDATION_ERROR";
                title = "Validation failed";
                errors = fvex.Errors
                    .GroupBy(f => f.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());
                break;
            default:
                if (env.IsDevelopment())
                    detail = ex.ToString();
                break;
        }

        var problem = new Dictionary<string, object?>
        {
            ["type"] = $"https://foodblog.local/errors/{(code ?? "INTERNAL_ERROR").ToLowerInvariant()}",
            ["title"] = title,
            ["status"] = (int)statusCode,
            ["detail"] = detail,
            ["code"] = code,
            ["traceId"] = context.TraceIdentifier
        };
        if (errors is not null)
            problem["errors"] = errors;

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
