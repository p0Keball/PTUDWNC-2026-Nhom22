namespace FoodBlog.Application.Common;

/// <summary>
/// Quy ước key các biến thể ảnh trên MinIO (FR-JOB-002).
/// Từ key gốc <c>recipes/{id}/{guid}.jpg</c> suy ra:
/// <c>recipes/{id}/{guid}_medium.jpg</c> (800×600) và
/// <c>recipes/{id}/{guid}_thumb.jpg</c> (300×300).
/// </summary>
public static class ImageVariants
{
    public const int MediumWidth = 800;
    public const int MediumHeight = 600;
    public const int ThumbWidth = 300;
    public const int ThumbHeight = 300;

    public static string MediumKey(string originalKey) => SuffixedKey(originalKey, "_medium");

    public static string ThumbKey(string originalKey) => SuffixedKey(originalKey, "_thumb");

    public static IEnumerable<string> AllKeys(string originalKey)
    {
        yield return originalKey;
        yield return MediumKey(originalKey);
        yield return ThumbKey(originalKey);
    }

    private static string SuffixedKey(string originalKey, string suffix)
    {
        var ext = Path.GetExtension(originalKey);
        var stem = originalKey[..^ext.Length];
        return $"{stem}{suffix}{ext}";
    }
}
