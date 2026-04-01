using Application.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // Register Repositories (pendiente implementación con EF Core)
        
        // Core
        services.AddScoped<IUserRepository, Repositories.UserRepository>();
        services.AddScoped<IPostRepository, Repositories.PostRepository>();
        
        // Social
        services.AddScoped<IFollowRepository, Repositories.FollowRepository>();
        services.AddScoped<ILikeRepository, Repositories.LikeRepository>();
        services.AddScoped<ICommentRepository, Repositories.CommentRepository>();
        services.AddScoped<IRetweetRepository, Repositories.RetweetRepository>();
        
        // Messaging
        services.AddScoped<IMessageRepository, Repositories.MessageRepository>();

        return services;
    }
}