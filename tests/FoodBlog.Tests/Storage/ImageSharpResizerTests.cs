using FoodBlog.Infrastructure.Services;
using FoodBlog.Tests.Infrastructure;
using SixLabors.ImageSharp;
using Xunit;

namespace FoodBlog.Tests.Storage;

public sealed class ImageSharpResizerTests
{
    [Fact]
    public async Task ResizeAsync_CreatesMedium800x600AndThumbnail300x300()
    {
        var resizer = new ImageSharpResizer();
        await using var source = new MemoryStream(TestImages.Jpeg(1600, 1200));

        var resized = await resizer.ResizeAsync(source);

        await using var medium = resized.Medium;
        await using var thumb = resized.Thumbnail;

        using var mediumImage = await Image.LoadAsync(medium);
        using var thumbImage = await Image.LoadAsync(thumb);

        Assert.Equal(800, mediumImage.Width);
        Assert.Equal(600, mediumImage.Height);
        Assert.Equal(300, thumbImage.Width);
        Assert.Equal(300, thumbImage.Height);
    }

    [Fact]
    public async Task ResizeAsync_OutputsJpegMagicBytes()
    {
        var resizer = new ImageSharpResizer();
        await using var source = new MemoryStream(TestImages.Png(640, 480));

        var resized = await resizer.ResizeAsync(source);
        await using var medium = resized.Medium;
        await using var thumb = resized.Thumbnail;

        using var buffer = new MemoryStream();
        await medium.CopyToAsync(buffer);

        var bytes = buffer.ToArray();
        Assert.True(bytes.Length > 3);
        Assert.Equal(0xFF, bytes[0]);
        Assert.Equal(0xD8, bytes[1]);
        Assert.Equal(0xFF, bytes[2]);
    }

    [Fact]
    public async Task ResizeAsync_InvalidImage_Throws()
    {
        var resizer = new ImageSharpResizer();
        await using var source = new MemoryStream([1, 2, 3, 4, 5]);

        await Assert.ThrowsAnyAsync<Exception>(() => resizer.ResizeAsync(source));
    }
}
