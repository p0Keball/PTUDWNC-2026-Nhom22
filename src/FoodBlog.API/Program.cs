using FoodBlog.API.Endpoints;
using FoodBlog.Domain.Entities;
using FoodBlog.Domain.Exceptions;
using FoodBlog.Infrastructure.Persistence;
using FoodBlog.Infrastructure.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddDbContext<FoodBlogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<FoodBlogDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

app.UseForwardedHeaders();
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var (statusCode, body) = exception switch
    {
        ValidationException validation => (StatusCodes.Status422UnprocessableEntity,
            (object)new { title = validation.Message, errors = validation.Errors }),
        NotFoundException notFound => (StatusCodes.Status404NotFound,
            (object)new { title = notFound.Message }),
        ConflictException conflict => (StatusCodes.Status409Conflict,
            (object)new { title = conflict.Message }),
        UnauthorizedException unauthorized => (StatusCodes.Status401Unauthorized,
            (object)new { title = unauthorized.Message }),
        DomainException domain => (StatusCodes.Status400BadRequest,
            (object)new { title = domain.Message }),
        _ => (StatusCodes.Status500InternalServerError,
            (object)new { title = "Đã xảy ra lỗi không mong muốn." })
    };

    await Results.Json(body, statusCode: statusCode).ExecuteAsync(context);
}));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FoodBlogDbContext>();
    await db.Database.MigrateAsync();
    await FoodBlogSeeder.SeedAsync(app.Services);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", async (FoodBlogDbContext db) =>
    await db.Database.CanConnectAsync() ? Results.Ok("Healthy") : Results.Problem("Unhealthy"))
    .WithName("HealthCheck");

app.MapCategoryEndpoints();
app.MapRecipeEndpoints();
app.MapRecipeChildEndpoints();

app.Run();
