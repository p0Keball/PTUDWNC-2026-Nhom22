using System.Reflection;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace FoodBlog.Application;

/// <summary>
/// Đăng ký DI cho Application Layer — Program.cs chỉ cần gọi AddApplication().
/// Yêu cầu packages (Yêu cầu 2): MediatR, Mapster, Mapster.DependencyInjection.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}
