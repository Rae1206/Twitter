using Application.Interfaces.Services;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Servicios
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ISuspensionService, SuspensionService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IConfigService, ConfigService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
