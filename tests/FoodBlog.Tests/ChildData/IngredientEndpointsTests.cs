using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FoodBlog.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FoodBlog.Tests.ChildData;

public sealed class IngredientEndpointsTests : ApiTestBase
{
    private static async Task<JsonElement> CreateIngredientAsync(
        HttpClient client, Guid recipeId, string name, decimal quantity = 1, string unit = "g")
    {
        var response = await client.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/ingredients", new
        {
            name,
            quantity,
            unit,
            notes = (string?)null,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    [Fact]
    public async Task Create_AssignsIncreasingOrderIndex()
    {
        var recipeId = await CreateRecipeAsync();

        var first = await CreateIngredientAsync(Client, recipeId, "Hành tím", 2, "củ");
        var second = await CreateIngredientAsync(Client, recipeId, "Gừng", 10, "g");

        Assert.Equal(0, first.GetProperty("orderIndex").GetInt32());
        Assert.Equal(1, second.GetProperty("orderIndex").GetInt32());
    }

    [Fact]
    public async Task Create_DuplicateName_ReturnsUnprocessableEntity()
    {
        var recipeId = await CreateRecipeAsync();
        await CreateIngredientAsync(Client, recipeId, "Hành tím");

        var response = await Client.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/ingredients", new
        {
            name = "  hành TÍM ",
            quantity = 1,
            unit = "củ",
            notes = (string?)null,
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("đã có trong danh sách", body.GetProperty("errors").GetProperty("name")[0].GetString());
    }

    [Fact]
    public async Task Update_DuplicateName_ReturnsUnprocessableEntity()
    {
        var recipeId = await CreateRecipeAsync();
        await CreateIngredientAsync(Client, recipeId, "Hành tím");
        var ginger = await CreateIngredientAsync(Client, recipeId, "Gừng");

        var response = await Client.PutAsJsonAsync(
            $"/api/v1/recipes/{recipeId}/ingredients/{ginger.GetProperty("id").GetGuid()}",
            new
            {
                name = "Hành tím",
                quantity = 5,
                unit = "g",
                notes = (string?)null,
            });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Delete_MiddleIngredient_RenumbersOrderIndex()
    {
        var recipeId = await CreateRecipeAsync();
        await CreateIngredientAsync(Client, recipeId, "Hành tím");
        var middle = await CreateIngredientAsync(Client, recipeId, "Gừng");
        await CreateIngredientAsync(Client, recipeId, "Quế");

        var delete = await Client.DeleteAsync(
            $"/api/v1/recipes/{recipeId}/ingredients/{middle.GetProperty("id").GetGuid()}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var orderIndexes = await TestData.QueryAsync(Factory.Services,
            db => db.RecipeIngredients
                .Where(i => i.RecipeId == recipeId)
                .OrderBy(i => i.OrderIndex)
                .Select(i => i.OrderIndex)
                .ToListAsync());

        Assert.Equal([0, 1], orderIndexes);
    }

    [Fact]
    public async Task Create_InvalidQuantity_ReturnsUnprocessableEntity()
    {
        var recipeId = await CreateRecipeAsync();

        var response = await Client.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/ingredients", new
        {
            name = "Muối",
            quantity = 0,
            unit = "g",
            notes = (string?)null,
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("errors").TryGetProperty("quantity", out _));
    }

    [Fact]
    public async Task Reorder_PersistsOrderIndex()
    {
        var recipeId = await CreateRecipeAsync();
        var one = await CreateIngredientAsync(Client, recipeId, "Hành tím");
        var two = await CreateIngredientAsync(Client, recipeId, "Gừng");

        var response = await Client.PatchAsJsonAsync($"/api/v1/recipes/{recipeId}/ingredients/reorder", new[]
        {
            new { id = two.GetProperty("id").GetGuid(), order = 0 },
            new { id = one.GetProperty("id").GetGuid(), order = 1 },
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var reordered = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Gừng", reordered[0].GetProperty("name").GetString());
        Assert.Equal(0, reordered[0].GetProperty("orderIndex").GetInt32());
    }
}
