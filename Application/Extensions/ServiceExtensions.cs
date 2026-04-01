using Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, Services.UserService>();
        services.AddScoped<IPostService, Services.PostService>();

        return services;
    }
}