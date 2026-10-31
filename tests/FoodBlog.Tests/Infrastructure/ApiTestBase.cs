using Xunit;

namespace FoodBlog.Tests.Infrastructure;

/// <summary>
/// Base cho integration test HTTP: mỗi test class có <see cref="FoodBlogApiFactory"/> + DB InMemory riêng,
/// client dùng chung trong class và được dispose cùng factory.
/// </summary>
public abstract class ApiTestBase : IAsyncLifetime
{
    protected FoodBlogApiFactory Factory { get; } = new();
    protected HttpClient Client { get; private set; } = default!;

    public Task InitializeAsync()
    {
        Client = Factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
    }

    protected Task<Guid> CreateRecipeAsync() => TestData.SeedRecipeAsync(Factory.Services);
}
