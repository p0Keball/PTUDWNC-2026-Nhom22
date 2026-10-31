using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FoodBlog.Infrastructure.Persistence;
using FoodBlog.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FoodBlog.Tests.ChildData;

public sealed class StepEndpointsTests : ApiTestBase
{
    private static async Task<JsonElement> CreateStepAsync(
        HttpClient client, Guid recipeId, string title, int? timerMinutes = null)
    {
        var response = await client.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/steps", new
        {
            title,
            description = $"Mô tả cho {title}",
            timerMinutes,
            imageUrl = (string?)null,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    [Fact]
    public async Task Create_AssignsSequentialStepNumbers()
    {
        var recipeId = await CreateRecipeAsync();

        var first = await CreateStepAsync(Client, recipeId, "Sơ chế nguyên liệu");
        var second = await CreateStepAsync(Client, recipeId, "Nấu nước dùng");

        Assert.Equal(1, first.GetProperty("stepNumber").GetInt32());
        Assert.Equal(2, second.GetProperty("stepNumber").GetInt32());
        Assert.Equal(2, await TestData.QueryAsync(Factory.Services,
            db => db.RecipeSteps.CountAsync(s => s.RecipeId == recipeId)));
    }

    [Fact]
    public async Task Delete_MiddleStep_RenumbersRemainingSteps()
    {
        var recipeId = await CreateRecipeAsync();
        await CreateStepAsync(Client, recipeId, "Bước một");
        var middle = await CreateStepAsync(Client, recipeId, "Bước hai");
        await CreateStepAsync(Client, recipeId, "Bước ba");

        var delete = await Client.DeleteAsync(
            $"/api/v1/recipes/{recipeId}/steps/{middle.GetProperty("id").GetGuid()}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var numbers = await TestData.QueryAsync(Factory.Services,
            db => db.RecipeSteps
                .Where(s => s.RecipeId == recipeId)
                .OrderBy(s => s.StepNumber)
                .Select(s => s.StepNumber)
                .ToListAsync());

        Assert.Equal([1, 2], numbers);
    }

    [Fact]
    public async Task Reorder_PersistsNewStepNumbers()
    {
        var recipeId = await CreateRecipeAsync();
        var one = await CreateStepAsync(Client, recipeId, "Bước một");
        var two = await CreateStepAsync(Client, recipeId, "Bước hai");
        var three = await CreateStepAsync(Client, recipeId, "Bước ba");

        var response = await Client.PatchAsJsonAsync($"/api/v1/recipes/{recipeId}/steps/reorder", new[]
        {
            new { id = three.GetProperty("id").GetGuid(), order = 1 },
            new { id = one.GetProperty("id").GetGuid(), order = 2 },
            new { id = two.GetProperty("id").GetGuid(), order = 3 },
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var reordered = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(3, reordered.GetArrayLength());
        Assert.Equal("Bước ba", reordered[0].GetProperty("title").GetString());
        Assert.Equal(1, reordered[0].GetProperty("stepNumber").GetInt32());
    }

    [Fact]
    public async Task Create_InvalidTitle_ReturnsUnprocessableEntity()
    {
        var recipeId = await CreateRecipeAsync();

        var response = await Client.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/steps", new
        {
            title = "ab",
            description = "quá ngắn",
            timerMinutes = (int?)null,
            imageUrl = (string?)null,
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("errors").TryGetProperty("title", out _));
    }

    [Fact]
    public async Task Update_ChangesStepContent()
    {
        var recipeId = await CreateRecipeAsync();
        var step = await CreateStepAsync(Client, recipeId, "Bước cũ", timerMinutes: 5);

        var response = await Client.PutAsJsonAsync(
            $"/api/v1/recipes/{recipeId}/steps/{step.GetProperty("id").GetGuid()}",
            new
            {
                title = "Bước mới",
                description = "Đã cập nhật",
                timerMinutes = 12,
                imageUrl = (string?)null,
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Bước mới", body.GetProperty("title").GetString());
        Assert.Equal(12, body.GetProperty("timerMinutes").GetInt32());
    }
}
