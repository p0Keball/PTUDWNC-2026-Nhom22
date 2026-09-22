namespace CulinaryBlog.Infrastructure;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Đăng ký các dịch vụ của Infrastructure Layer (DbContext, Repositories, Identity, v.v.)
        
        return services;
    }
}