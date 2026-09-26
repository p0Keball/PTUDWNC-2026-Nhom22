using FoodBlog.API.Endpoints;
using FoodBlog.Domain.Entities;
using FoodBlog.Infrastructure.Persistence;
using FoodBlog.Infrastructure.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();
builder.Services.AddDbContext<FoodBlogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<FoodBlogDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

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

app.Run();
