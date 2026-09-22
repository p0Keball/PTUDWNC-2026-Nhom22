using System.Text;
using System.Text.RegularExpressions;

namespace FoodBlog.Domain.Common;

/// <summary>
/// Sinh URL slug thân thiện SEO: "Món Tráng Miệng" -> "mon-trang-mieng".
/// Dùng cho Category (FR-CAT-003) và Recipe (FR-RCP-003).
/// Production có thể thay bằng package Slugify.Core.
/// </summary>
public static class SlugHelper
{
    public static string Generate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Chuẩn hóa Unicode, tách dấu (FormD) rồi loại bỏ dấu
        var normalized = input.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var withoutDiacritics = sb.ToString().Normalize(NormalizationForm.FormC)
            .Replace('đ', 'd');

        // Thay khoảng trắng/ký tự đặc biệt bằng gạch ngang
        var slug = Regex.Replace(withoutDiacritics, @"[^a-z0-9\s-]", string.Empty);
        slug = Regex.Replace(slug, @"[\s_]+", "-");
        slug = Regex.Replace(slug, @"-+", "-").Trim('-');

        return slug;
    }
}
