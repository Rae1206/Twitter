using System.Security.Claims;
using System.Text;
using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer;
using Twitter.Domain.Interfaces.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Application.Interfaces.Services;
using Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Shared;
using Shared.Constants;

namespace WebApi.Extensions;

/// <summary>
/// Extensiones para configurar el contenedor de dependencias.
/// Punto central de configuración de toda la infraestructura según Clean Architecture.
/// </summary>
public static class ServiceCollectionExtension
{
    /// <summary>
    /// Configura TODA la infraestructura del proyecto: DbContext, Cache, Repositorios, Servicios, JWT.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. DbContext con SQL Server ( Render用__)
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("CONNECTION_STRING_DATABASE");

        services.AddDbContext<TwitterDbContext>(options =>
            options.UseSqlServer(connectionString));

        // 2. Memory Cache
        services.AddMemoryCache();

        // 3. Cache Service
        services.AddSingleton<ICacheService, CacheService>();

        // 4. Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // 5. Repositorios
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IEmailTemplateRepository, EmailTemplateRepository>();

        // 6. Servicios de Application
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPostService, PostService>();

        // 7. Email
        services.AddSingleton<SMTP>();
        services.AddScoped<IEmailService>(sp => new EmailService(
            sp.GetRequiredService<SMTP>(),
            sp.GetRequiredService<IServiceScopeFactory>(),
            sp.GetRequiredService<ILogger<EmailService>>()
        ));

        // 8. JWT Authentication
        AddJwtAuthentication(services, configuration);

        return services;
    }

    /// <summary>
    /// Configura la autenticación JWT.
    /// </summary>
    private static void AddJwtAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        // Cargar configuración JWT -优先环境变量 ( Render用__)
        var issuer = Environment.GetEnvironmentVariable("Jwt__Issuer")
            ?? Environment.GetEnvironmentVariable(ConfigurationConstants.JWT_ISSUER)
            ?? configuration[ConfigurationConstants.JWT_ISSUER]
            ?? throw new InvalidOperationException("JWT Issuer no configurado");

        var audience = Environment.GetEnvironmentVariable("Jwt__Audience")
            ?? Environment.GetEnvironmentVariable(ConfigurationConstants.JWT_AUDIENCE)
            ?? configuration[ConfigurationConstants.JWT_AUDIENCE]
            ?? throw new InvalidOperationException("JWT Audience no configurado");

        var privateKey = Environment.GetEnvironmentVariable("Jwt__PrivateKey")
            ?? Environment.GetEnvironmentVariable(ConfigurationConstants.JWT_PRIVATE_KEY)
            ?? configuration[ConfigurationConstants.JWT_PRIVATE_KEY]
            ?? throw new InvalidOperationException("JWT PrivateKey no configurado");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = securityKey,
                ClockSkew = TimeSpan.Zero,

                // Validar roles desde el token
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.Name
            };
        });

        services.AddAuthorization();
    }
}