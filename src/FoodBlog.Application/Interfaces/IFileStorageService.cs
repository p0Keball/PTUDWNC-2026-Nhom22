using Microsoft.AspNetCore.Http;

namespace FoodBlog.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct = default);
    Task DeleteAsync(string fileUrl, CancellationToken ct = default);
}