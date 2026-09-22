using FoodBlog.API.Endpoints;
using FoodBlog.Application;
using FoodBlog.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Clean Architecture DI (§1.2.3, §1.5.1)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// OpenAPI native .NET 10 (§1.4.1) — không dùng Swashbuckle
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info = new()
        {
            Title = "FoodBlog API",
            Version = "v1",
            Description = "API Blog ẩm thực và nấu ăn — .NET 10 Minimal APIs + Clean Architecture",
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("FoodBlog API")
            .WithTheme(ScalarTheme.Purple)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();

// Minimal API route groups (§1.3.4 versioning /api/v1)
app.MapCategoryEndpoints();

app.Run();
