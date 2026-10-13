using FoodBlog.Application.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace FoodBlog.Infrastructure.Services;

/// <summary>Resize ảnh bằng ImageSharp (FR-JOB-002).</summary>
public sealed class ImageSharpResizer : IImageResizer
{
    private static readonly JpegEncoder Encoder = new() { Quality = 80 };

    public async Task<ResizedImages> ResizeAsync(Stream source, CancellationToken ct = default)
    {
        using var image = await Image.LoadAsync(source, ct);

        var medium = new MemoryStream();
        var thumb = new MemoryStream();
        try
        {
            using (var clone = image.Clone(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(800, 600),
                Mode = ResizeMode.Crop,
            })))
            {
                await clone.SaveAsJpegAsync(medium, Encoder, ct);
            }
            medium.Position = 0;

            using (var clone = image.Clone(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(300, 300),
                Mode = ResizeMode.Crop,
            })))
            {
                await clone.SaveAsJpegAsync(thumb, Encoder, ct);
            }
            thumb.Position = 0;

            return new ResizedImages(medium, thumb);
        }
        catch
        {
            await medium.DisposeAsync();
            await thumb.DisposeAsync();
            throw;
        }
    }
}
