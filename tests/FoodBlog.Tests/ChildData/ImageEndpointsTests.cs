using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FoodBlog.Application.Common;
using FoodBlog.Domain.Entities;
using FoodBlog.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FoodBlog.Tests.ChildData;

public sealed class ImageEndpointsTests : ApiTestBase
{
    private async Task<JsonElement> UploadAsync(
        Guid recipeId,
        byte[] bytes,
        string fileName = "photo.png",
        string contentType = "image/png",
        string? altText = null)
    {
        using var content = new MultipartFormDataContent();
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(file, "file", fileName);

        var url = $"/api/v1/recipes/{recipeId}/images";
        if (altText is not null)
            url += $"?altText={Uri.EscapeDataString(altText)}";

        var response = await Client.PostAsync(url, content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static MultipartFormDataContent BuildMultipart(
        byte[] bytes, string fileName, string contentType)
    {
        var content = new MultipartFormDataContent();
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(file, "file", fileName);
        return content;
    }

    private Task<List<RecipeImage>> ImagesAsync(Guid recipeId)
        => TestData.QueryAsync(Factory.Services, db => db.RecipeImages
            .Where(i => i.RecipeId == recipeId)
            .OrderBy(i => i.OrderIndex)
            .ToListAsync());

    [Fact]
    public async Task Upload_FirstImage_IsPrimary_AndEnqueuesThumbnailJob()
    {
        var recipeId = await CreateRecipeAsync();

        var image = await UploadAsync(recipeId, TestImages.Png(), altText: "Ảnh bìa");

        Assert.True(image.GetProperty("isPrimary").GetBoolean());
        Assert.Equal("pending", image.GetProperty("thumbnailStatus").GetString());
        Assert.Equal("Ảnh bìa", image.GetProperty("altText").GetString());

        var objectKey = Factory.Storage.TryExtractKey(image.GetProperty("originalUrl").GetString()!);
        Assert.NotNull(objectKey);
        Assert.StartsWith($"recipes/{recipeId}/", objectKey);
        Assert.True(Factory.Storage.Contains(objectKey!));

        Assert.True(Factory.Jobs.ThumbnailJobs.TryDequeue(out var queuedImageId));
        Assert.Equal(image.GetProperty("id").GetGuid(), queuedImageId);
    }

    [Fact]
    public async Task Upload_SecondImage_IsNotPrimary()
    {
        var recipeId = await CreateRecipeAsync();
        await UploadAsync(recipeId, TestImages.Png());
        var second = await UploadAsync(recipeId, TestImages.Png());

        Assert.False(second.GetProperty("isPrimary").GetBoolean());
        Assert.Equal(1, second.GetProperty("orderIndex").GetInt32());
    }

    [Fact]
    public async Task Upload_MimeDoesNotMatchMagicBytes_ReturnsBadRequest()
    {
        var recipeId = await CreateRecipeAsync();
        using var content = BuildMultipart(TestImages.Jpeg(), "fake.png", "image/png");

        var response = await Client.PostAsync($"/api/v1/recipes/{recipeId}/images", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("FILE_MIME_INVALID", body.GetProperty("code").GetString());
        Assert.Empty(await ImagesAsync(recipeId));
    }

    [Fact]
    public async Task Upload_UnsupportedMimeType_ReturnsBadRequest()
    {
        var recipeId = await CreateRecipeAsync();
        using var content = BuildMultipart([1, 2, 3], "note.txt", "text/plain");

        var response = await Client.PostAsync($"/api/v1/recipes/{recipeId}/images", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("FILE_MIME_INVALID", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Upload_FileLargerThanFiveMegabytes_ReturnsBadRequest()
    {
        var recipeId = await CreateRecipeAsync();
        var oversized = new byte[ImageValidation.MaxFileSizeBytes + 1];
        TestImages.PngHeader().CopyTo(oversized, 0);
        using var content = BuildMultipart(oversized, "big.png", "image/png");

        var response = await Client.PostAsync($"/api/v1/recipes/{recipeId}/images", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("FILE_SIZE_EXCEEDED", body.GetProperty("code").GetString());
        Assert.False(Factory.Jobs.ThumbnailJobs.TryDequeue(out _));
    }

    [Fact]
    public async Task SetPrimary_UpdatesOnlyOneImage()
    {
        var recipeId = await CreateRecipeAsync();
        var first = await UploadAsync(recipeId, TestImages.Png());
        var second = await UploadAsync(recipeId, TestImages.Png());

        var response = await Client.PatchAsync(
            $"/api/v1/recipes/{recipeId}/images/{second.GetProperty("id").GetGuid()}/primary", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var images = await ImagesAsync(recipeId);
        var primary = Assert.Single(images, i => i.IsPrimary);
        Assert.Equal(second.GetProperty("id").GetGuid(), primary.Id);
        Assert.NotEqual(first.GetProperty("id").GetGuid(), primary.Id);
    }

    [Fact]
    public async Task Delete_PrimaryImage_PromotesNextAndRenumbers()
    {
        var recipeId = await CreateRecipeAsync();
        var first = await UploadAsync(recipeId, TestImages.Png());
        var second = await UploadAsync(recipeId, TestImages.Png());
        await UploadAsync(recipeId, TestImages.Png());

        await Client.PatchAsync(
            $"/api/v1/recipes/{recipeId}/images/{second.GetProperty("id").GetGuid()}/primary", null);

        var delete = await Client.DeleteAsync(
            $"/api/v1/recipes/{recipeId}/images/{second.GetProperty("id").GetGuid()}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var images = await ImagesAsync(recipeId);
        Assert.Equal(2, images.Count);
        Assert.Equal(first.GetProperty("id").GetGuid(), images.Single(i => i.IsPrimary).Id);
        Assert.Equal([0, 1], images.Select(i => i.OrderIndex));
    }

    [Fact]
    public async Task Delete_Image_RemovesOriginalMediumAndThumbnailFromStorage()
    {
        var recipeId = await CreateRecipeAsync();
        var image = await UploadAsync(recipeId, TestImages.Png());
        var originalKey = Factory.Storage.TryExtractKey(
            image.GetProperty("originalUrl").GetString()!)!;

        var mediumKey = ImageVariants.MediumKey(originalKey);
        var thumbKey = ImageVariants.ThumbKey(originalKey);
        Factory.Storage.PutObject(mediumKey, TestImages.Jpeg(800, 600));
        Factory.Storage.PutObject(thumbKey, TestImages.Jpeg(300, 300));

        var delete = await Client.DeleteAsync(
            $"/api/v1/recipes/{recipeId}/images/{image.GetProperty("id").GetGuid()}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        var deleted = Factory.Storage.DeletedKeys.ToArray();
        Assert.Contains(originalKey, deleted);
        Assert.Contains(mediumKey, deleted);
        Assert.Contains(thumbKey, deleted);
    }

    [Fact]
    public async Task Presign_ValidImage_ReturnsUploadAndPublicUrl()
    {
        var recipeId = await CreateRecipeAsync();

        var response = await Client.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/images/presign", new
        {
            fileName = "cover.jpg",
            contentType = "image/jpeg",
            fileSize = 1024,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Contains("sig=test", body.GetProperty("uploadUrl").GetString());
        Assert.StartsWith($"recipes/{recipeId}/", body.GetProperty("objectKey").GetString());
        Assert.EndsWith(".jpg", body.GetProperty("objectKey").GetString());
    }

    [Fact]
    public async Task Presign_UnsupportedContentType_ReturnsBadRequest()
    {
        var recipeId = await CreateRecipeAsync();

        var response = await Client.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/images/presign", new
        {
            fileName = "document.pdf",
            contentType = "application/pdf",
            fileSize = 1024,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("FILE_MIME_INVALID", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Confirm_MissingObject_ReturnsUnprocessableEntity()
    {
        var recipeId = await CreateRecipeAsync();

        var response = await Client.PostAsJsonAsync($"/api/v1/recipes/{recipeId}/images/confirm", new
        {
            objectKey = $"recipes/{recipeId}/missing.jpg",
            altText = (string?)null,
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }
}
