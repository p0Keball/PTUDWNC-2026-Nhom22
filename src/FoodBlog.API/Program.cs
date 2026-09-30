using FoodBlog.API.Endpoints;
using FoodBlog.Application;
using FoodBlog.Domain.Entities;
using FoodBlog.Infrastructure.Persistence;
using FoodBlog.Infrastructure.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();
builder.Services.AddApplication();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddDbContext<FoodBlogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<FoodBlogDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

app.UseForwardedHeaders();

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
