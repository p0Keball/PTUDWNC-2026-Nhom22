namespace CulinaryBlog.Application;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Đăng ký các dịch vụ của Application Layer (MediatR, AutoMapper, FluentValidation, v.v.)
        
        return services;
    }
}