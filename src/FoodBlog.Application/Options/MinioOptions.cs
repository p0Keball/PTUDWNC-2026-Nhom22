namespace FoodBlog.Application.Options;

public sealed class MinioOptions
{
    public const string SectionName = "MinIO";

    public string Endpoint { get; set; } = "localhost:9000";
    public string AccessKey { get; set; } = "minioadmin";
    public string SecretKey { get; set; } = "minioadmin123";
    public string Bucket { get; set; } = "culinary-blog";
    public string PublicUrl { get; set; } = "http://localhost:9000";
    public bool UseSsl { get; set; } = false;
}
