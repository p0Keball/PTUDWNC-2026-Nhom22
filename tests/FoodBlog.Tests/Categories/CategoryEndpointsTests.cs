using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FoodBlog.Domain.Entities;
using FoodBlog.Infrastructure.Persistence;
using FoodBlog.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FoodBlog.Tests.Categories;

public sealed class CategoryEndpointsTests : ApiTestBase
{
    private static void AsAdmin(HttpClient client)
    {
        client.DefaultRequestHeaders.Remove("X-Test-Role");
        client.DefaultRequestHeaders.Remove("X-Test-Anonymous");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
    }

    private static void AsGuest(HttpClient client)
    {
        client.DefaultRequestHeaders.Remove("X-Test-Role");
        client.DefaultRequestHeaders.Remove("X-Test-Anonymous");
        client.DefaultRequestHeaders.Add("X-Test-Anonymous", "true");
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithRecipeCounts()
    {
        await TestData.SeedRecipeAsync(Factory.Services);
        AsGuest(Client);

        var response = await Client.GetAsync("/api/v1/categories");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var categories = await response.Content.ReadFromJsonAsync<List<JsonElement>>();
        Assert.NotNull(categories);
        Assert.NotEmpty(categories);
        var first = categories[0];
        Assert.True(first.TryGetProperty("id", out _));
        Assert.True(first.TryGetProperty("name", out _));
        Assert.True(first.TryGetProperty("recipeCount", out var countProp));
        Assert.True(countProp.GetInt32() >= 1);
    }

    [Fact]
    public async Task GetBySlug_NonExistent_Returns404()
    {
        AsGuest(Client);
        var response = await Client.GetAsync("/api/v1/categories/non-existent-category-slug");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetBySlug_Existing_ReturnsCategoryAndRecipes()
    {
        await TestData.SeedRecipeAsync(Factory.Services);
        string slug = await TestData.QueryAsync(Factory.Services,
            async db => (await db.Categories.FirstAsync()).Slug);

        AsGuest(Client);
        var response = await Client.GetAsync($"/api/v1/categories/{slug}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(json.TryGetProperty("category", out var categoryProp));
        Assert.Equal(slug, categoryProp.GetProperty("slug").GetString());
        Assert.True(json.TryGetProperty("recipes", out var recipesProp));
        Assert.True(recipesProp.GetProperty("totalCount").GetInt32() >= 1);
    }

    [Fact]
    public async Task Create_AsNonAdmin_Returns403()
    {
        // Default client role is Author (not Admin)
        var response = await Client.PostAsJsonAsync("/api/v1/categories", new
        {
            name = "Món Tráng Miệng",
            description = "Các món ngọt hấp dẫn"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_AsAdmin_CreatesCategory_AndGeneratesSlug()
    {
        AsAdmin(Client);

        var response = await Client.PostAsJsonAsync("/api/v1/categories", new
        {
            name = "Món Bánh Mì",
            description = "Bánh mì Việt Nam và thế giới"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.NotNull(created);
        Assert.Equal("Món Bánh Mì", created.GetProperty("name").GetString());
        Assert.Equal("mon-banh-mi", created.GetProperty("slug").GetString());

        var existsInDb = await TestData.QueryAsync(Factory.Services,
            db => db.Categories.AnyAsync(c => c.Slug == "mon-banh-mi"));
        Assert.True(existsInDb);
    }

    [Fact]
    public async Task Create_DuplicateName_Returns409Conflict()
    {
        AsAdmin(Client);

        var first = await Client.PostAsJsonAsync("/api/v1/categories", new
        {
            name = "Món Lẩu",
            description = "Các món lẩu"
        });
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var duplicate = await Client.PostAsJsonAsync("/api/v1/categories", new
        {
            name = "Món Lẩu",
            description = "Lẩu trùng tên"
        });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public async Task Update_AsAdmin_UpdatesCategoryDetails()
    {
        AsAdmin(Client);

        var createRes = await Client.PostAsJsonAsync("/api/v1/categories", new
        {
            name = "Món Xào Gốc",
            description = "Mô tả cũ"
        });
        var created = await createRes.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetGuid();

        var updateRes = await Client.PutAsJsonAsync($"/api/v1/categories/{id}", new
        {
            name = "Món Xào Cập Nhật",
            description = "Mô tả mới"
        });
        Assert.Equal(HttpStatusCode.OK, updateRes.StatusCode);

        var updated = await updateRes.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Món Xào Cập Nhật", updated.GetProperty("name").GetString());
        Assert.Equal("Mô tả mới", updated.GetProperty("description").GetString());
    }

    [Fact]
    public async Task Delete_WithRecipes_Returns409Conflict()
    {
        await TestData.SeedRecipeAsync(Factory.Services);
        var categoryId = await TestData.QueryAsync(Factory.Services,
            async db => (await db.Categories.FirstAsync()).Id);

        AsAdmin(Client);
        var response = await Client.DeleteAsync($"/api/v1/categories/{categoryId}");
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("CATEGORY_DELETE_HAS_RECIPES", body);
    }

    [Fact]
    public async Task Delete_EmptyCategory_Returns204NoContent()
    {
        AsAdmin(Client);
        var createRes = await Client.PostAsJsonAsync("/api/v1/categories", new
        {
            name = "Danh Mục Rỗng Cần Xóa",
            description = "Không có công thức nào"
        });
        var created = await createRes.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetGuid();

        var deleteRes = await Client.DeleteAsync($"/api/v1/categories/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);

        var exists = await TestData.QueryAsync(Factory.Services,
            db => db.Categories.AnyAsync(c => c.Id == id));
        Assert.False(exists);
    }
}
