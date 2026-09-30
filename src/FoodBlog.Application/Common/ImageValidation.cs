namespace FoodBlog.Application.Common;

public static class ImageValidation
{
    public const long MaxFileSizeBytes = 5 * 1024 * 1024;

    public static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/avif",
    };

    public static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".avif",
    };

    public static bool IsExtensionAllowed(string fileName)
        => AllowedExtensions.Contains(Path.GetExtension(fileName));

    public static bool IsContentTypeAllowed(string? contentType)
        => contentType is not null && AllowedContentTypes.Contains(contentType);

    /// <summary>
    /// Kiểm tra magic bytes: JPEG (FF D8 FF), PNG (89 50 4E 47),
    /// WebP (RIFF....WEBP), AVIF (....ftypavif / ftypavis).
    /// </summary>
    public static bool HasValidMagicBytes(ReadOnlySpan<byte> leadingBytes, string contentType)
    {
        if (leadingBytes.Length < 4)
            return false;

        if (contentType.Equals("image/png", StringComparison.OrdinalIgnoreCase))
            return leadingBytes[0] == 0x89 && leadingBytes[1] == 0x50
                && leadingBytes[2] == 0x4E && leadingBytes[3] == 0x47;

        if (contentType.Equals("image/webp", StringComparison.OrdinalIgnoreCase))
        {
            if (leadingBytes.Length < 12)
                return false;
            return leadingBytes[0] == 0x52 && leadingBytes[1] == 0x49
                && leadingBytes[2] == 0x46 && leadingBytes[3] == 0x46
                && leadingBytes[8] == 0x57 && leadingBytes[9] == 0x45
                && leadingBytes[10] == 0x42 && leadingBytes[11] == 0x50;
        }

        if (contentType.Equals("image/avif", StringComparison.OrdinalIgnoreCase))
        {
            // ISO BMFF: bytes 4-7 = "ftyp", bytes 8-11 = "avif" hoặc "avis".
            if (leadingBytes.Length < 12)
                return false;
            return leadingBytes[4] == 0x66 && leadingBytes[5] == 0x74
                && leadingBytes[6] == 0x79 && leadingBytes[7] == 0x70
                && leadingBytes[8] == 0x61 && leadingBytes[9] == 0x76
                && leadingBytes[10] == 0x69
                && (leadingBytes[11] == 0x66 || leadingBytes[11] == 0x73);
        }

        // image/jpeg
        return leadingBytes[0] == 0xFF && leadingBytes[1] == 0xD8 && leadingBytes[2] == 0xFF;
    }

    public static string BuildObjectKey(Guid recipeId, string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return $"recipes/{recipeId}/{Guid.NewGuid():N}{ext}";
    }
}
