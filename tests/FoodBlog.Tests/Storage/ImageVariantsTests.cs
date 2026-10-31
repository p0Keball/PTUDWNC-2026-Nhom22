using FoodBlog.Application.Common;
using Xunit;

namespace FoodBlog.Tests.Storage;

public sealed class ImageVariantsTests
{
    [Fact]
    public void MediumKey_InsertsSuffixBeforeExtension()
        => Assert.Equal("recipes/abc/photo_medium.jpg", ImageVariants.MediumKey("recipes/abc/photo.jpg"));

    [Fact]
    public void ThumbKey_InsertsSuffixBeforeExtension()
        => Assert.Equal("recipes/abc/photo_thumb.png", ImageVariants.ThumbKey("recipes/abc/photo.png"));

    [Fact]
    public void AllKeys_ReturnsOriginalMediumThumbnail()
    {
        var keys = ImageVariants.AllKeys("recipes/abc/photo.webp").ToArray();

        Assert.Equal(3, keys.Length);
        Assert.Equal("recipes/abc/photo.webp", keys[0]);
        Assert.Equal("recipes/abc/photo_medium.webp", keys[1]);
        Assert.Equal("recipes/abc/photo_thumb.webp", keys[2]);
    }

    [Fact]
    public void VariantDimensions_MatchSpec()
    {
        Assert.Equal(800, ImageVariants.MediumWidth);
        Assert.Equal(600, ImageVariants.MediumHeight);
        Assert.Equal(300, ImageVariants.ThumbWidth);
        Assert.Equal(300, ImageVariants.ThumbHeight);
    }
}
