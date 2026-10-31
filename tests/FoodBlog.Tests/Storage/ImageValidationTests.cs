using FoodBlog.Application.Common;
using FoodBlog.Tests.Infrastructure;
using Xunit;

namespace FoodBlog.Tests.Storage;

public sealed class ImageValidationTests
{
    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/webp")]
    [InlineData("image/avif")]
    [InlineData("IMAGE/PNG")]
    public void IsContentTypeAllowed_AcceptsSupportedTypes(string contentType)
        => Assert.True(ImageValidation.IsContentTypeAllowed(contentType));

    [Theory]
    [InlineData("image/gif")]
    [InlineData("image/svg+xml")]
    [InlineData("application/pdf")]
    [InlineData("")]
    [InlineData(null)]
    public void IsContentTypeAllowed_RejectsUnsupportedTypes(string? contentType)
        => Assert.False(ImageValidation.IsContentTypeAllowed(contentType));

    [Theory]
    [InlineData("photo.jpg", true)]
    [InlineData("photo.JPEG", true)]
    [InlineData("photo.png", true)]
    [InlineData("photo.webp", true)]
    [InlineData("photo.avif", true)]
    [InlineData("photo.gif", false)]
    [InlineData("photo.pdf", false)]
    [InlineData("photo", false)]
    public void IsExtensionAllowed_OnlyAllowsImageExtensions(string fileName, bool expected)
        => Assert.Equal(expected, ImageValidation.IsExtensionAllowed(fileName));

    [Fact]
    public void HasValidMagicBytes_AcceptsPng()
        => Assert.True(ImageValidation.HasValidMagicBytes(TestImages.PngHeader(), "image/png"));

    [Fact]
    public void HasValidMagicBytes_AcceptsJpeg()
        => Assert.True(ImageValidation.HasValidMagicBytes(TestImages.JpegHeader(), "image/jpeg"));

    [Fact]
    public void HasValidMagicBytes_AcceptsWebp()
        => Assert.True(ImageValidation.HasValidMagicBytes(TestImages.WebpHeader(), "image/webp"));

    [Fact]
    public void HasValidMagicBytes_AcceptsAvif()
        => Assert.True(ImageValidation.HasValidMagicBytes(TestImages.AvifHeader(), "image/avif"));

    [Fact]
    public void HasValidMagicBytes_RejectsMismatchedMime()
    {
        // Nội dung JPEG nhưng khai báo PNG — đúng kiểu tấn công đổi đuôi file.
        Assert.False(ImageValidation.HasValidMagicBytes(TestImages.JpegHeader(), "image/png"));
        Assert.False(ImageValidation.HasValidMagicBytes(TestImages.PngHeader(), "image/jpeg"));
    }

    [Fact]
    public void HasValidMagicBytes_RejectsTooShortPayload()
    {
        Assert.False(ImageValidation.HasValidMagicBytes([0x89, 0x50], "image/png"));
        Assert.False(ImageValidation.HasValidMagicBytes([], "image/jpeg"));
    }

    [Fact]
    public void HasValidMagicBytes_RejectsRandomBytes()
        => Assert.False(ImageValidation.HasValidMagicBytes([0x00, 0x01, 0x02, 0x03, 0x04, 0x05], "image/png"));

    [Fact]
    public void MaxFileSize_IsFiveMegabytes()
        => Assert.Equal(5L * 1024 * 1024, ImageValidation.MaxFileSizeBytes);

    [Fact]
    public void BuildObjectKey_UsesRecipeFolderAndPreservesExtension()
    {
        var recipeId = Guid.NewGuid();

        var key = ImageValidation.BuildObjectKey(recipeId, "Ảnh Bìa.PNG");

        Assert.StartsWith($"recipes/{recipeId}/", key);
        Assert.EndsWith(".png", key);
        Assert.Equal(32 + $"recipes/{recipeId}/".Length + 4, key.Length);
    }
}
