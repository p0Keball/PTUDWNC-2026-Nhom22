using FoodBlog.Application.Features.Recipes.Queries;
using FoodBlog.Domain.Entities;
using FoodBlog.Domain.Enums;
using FoodBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FoodBlog.Tests.Recipes;

/// <summary>
/// FR-RCP-001 / FR-SRCH-001: quy tắc hiển thị công thức theo vai trò.
/// Guest chỉ thấy Published; Author thấy thêm Draft/Archived của chính mình;
/// Admin thấy tất cả.
/// </summary>
public sealed class RecipeVisibilityTests : IDisposable
{
    private const string AuthorA = "author-a";
    private const string AuthorB = "author-b";

    private readonly FoodBlogDbContext _db;

    public RecipeVisibilityTests()
    {
        var options = new DbContextOptionsBuilder<FoodBlogDbContext>()
            .UseInMemoryDatabase($"visibility-{Guid.NewGuid():N}")
            .Options;
        _db = new FoodBlogDbContext(options);
    }

    [Fact]
    public async Task Guest_SeesOnlyPublishedRecipes()
    {
        await SeedAsync();

        var result = await new GetRecipesQueryHandler(_db)
            .Handle(new GetRecipesQuery(PageSize: 50), CancellationToken.None);

        var titles = result.Items.Select(item => item.Title).OrderBy(title => title).ToList();
        Assert.Equal(new[] { "Published A", "Published B" }, titles);
    }

    [Fact]
    public async Task Author_SeesPublishedPlusOwnDraftAndArchived()
    {
        await SeedAsync();

        var result = await new GetRecipesQueryHandler(_db)
            .Handle(new GetRecipesQuery(PageSize: 50, CurrentUserId: AuthorA), CancellationToken.None);

        var titles = result.Items.Select(item => item.Title).OrderBy(title => title).ToList();
        Assert.Equal(new[] { "Archived A", "Draft A", "Published A", "Published B" }, titles);
    }

    [Fact]
    public async Task Admin_SeesAllRecipes()
    {
        await SeedAsync();

        var result = await new GetRecipesQueryHandler(_db)
            .Handle(new GetRecipesQuery(PageSize: 50, IsAdmin: true), CancellationToken.None);

        Assert.Equal(5, result.TotalCount);
    }

    [Fact]
    public async Task MinServings_FiltersRecipes()
    {
        await SeedAsync();

        var result = await new GetRecipesQueryHandler(_db)
            .Handle(new GetRecipesQuery(PageSize: 50, MinServings: 4), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(4, result.Items[0].Servings);
    }

    private async Task SeedAsync()
    {
        var category = new Category { Name = "Món Việt", Slug = "mon-viet" };
        _db.Categories.Add(category);

        _db.Recipes.AddRange(
            CreateRecipe("Published A", AuthorA, RecipeStatus.Published, servings: 4),
            CreateRecipe("Draft A", AuthorA, RecipeStatus.Draft, servings: 2),
            CreateRecipe("Archived A", AuthorA, RecipeStatus.Archived, servings: 2),
            CreateRecipe("Published B", AuthorB, RecipeStatus.Published, servings: 2),
            CreateRecipe("Draft B", AuthorB, RecipeStatus.Draft, servings: 2));

        await _db.SaveChangesAsync();
    }

    private static Recipe CreateRecipe(
        string title,
        string authorId,
        RecipeStatus status,
        int servings) => new()
    {
        Title = title,
        Slug = title.ToLowerInvariant().Replace(' ', '-'),
        Description = title,
        Instructions = title,
        CategoryId = Guid.NewGuid(),
        AuthorId = authorId,
        Status = status,
        Difficulty = Difficulty.Easy,
        PrepTimeMinutes = 10,
        CookTimeMinutes = 10,
        Servings = servings,
        PublishedAt = status == RecipeStatus.Published ? DateTime.UtcNow : null,
    };

    public void Dispose() => _db.Dispose();
}
