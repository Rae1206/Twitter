using System.Security.Claims;
using System.Text;
using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Interfaces;
using Twitter.Domain.Interfaces.Repositories;
using Twitter.Domain.Interfaces.Services;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Persistence.SqlServer.Repositories;
using Infrastructure.Persistence.Storage;
using Infrastructure.Background;
using Application.Interfaces.Services;
using Application.Services;
using Application.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using Shared;
using Shared.Constants;
using WebApi.Common;

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
        // 1. DbContext con SQL Server (Render usa __ como separador)
        var connectionString = FirstNonEmpty(
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection"),
            configuration.GetConnectionString("DefaultConnection"),
            Environment.GetEnvironmentVariable("CONNECTION_STRING_DATABASE"));

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("No SQL Server connection string has been configured.");
        }

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
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<IAdminAuditLogRepository, AdminAuditLogRepository>();
        services.AddScoped<IUserSuspensionRepository, UserSuspensionRepository>();
        services.AddScoped<IContentReportRepository, ContentReportRepository>();
        services.AddScoped<ISystemConfigRepository, SystemConfigRepository>();
        services.AddScoped<IAdminDashboardStatRepository, AdminDashboardStatRepository>();
        services.AddScoped<IAdminSessionRepository, AdminSessionRepository>();
        services.AddScoped<IPostMediaRepository, PostMediaRepository>();
        services.AddScoped<ILikeRepository, LikeRepository>();

        services.AddScoped<IFollowRepository, FollowRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IChatbotMessageRepository, ChatbotMessageRepository>();

        // 6. Servicios de Application
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPostService, PostService>();
        services.AddHttpClient<IPostTextGenerationService, GroqPostTextGenerationService>((serviceProvider, client) =>
        {
            var groqBaseUrl = configuration[ConfigurationConstants.GROQ_BASE_URL] ?? "https://api.groq.com/openai/v1/";
            client.BaseAddress = new Uri(groqBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ISuspensionService, SuspensionService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IConfigService, ConfigService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<ILikeService, LikeService>();
        services.AddScoped<IRetweetService, RetweetService>();
        services.AddScoped<IAvatarService, AvatarService>();
        services.AddScoped<IFollowService, FollowService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddHttpClient<IChatbotService, ChatbotService>((serviceProvider, client) =>
        {
            var groqBaseUrl = configuration[ConfigurationConstants.GROQ_BASE_URL] ?? "https://api.groq.com/openai/v1/";
            client.BaseAddress = new Uri(groqBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<UploadMediaRequestValidator>();

        // Selección de proveedor de almacenamiento: local (por defecto) o digitalocean
        var storageProvider = configuration["Storage:Provider"]?.ToLowerInvariant() ?? "local";
        if (storageProvider == "digitalocean")
        {
            services.AddSingleton<IMediaStorageService, SpacesStorageService>();
        }
        else
        {
            services.AddSingleton<IMediaStorageService, LocalFileStorageService>();
        }

        // 7. Background services
        services.AddHostedService<OrphanedMediaCleanupService>();
        services.AddHostedService<EphemeralPostCleanupService>();

        // 8. Email
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
        // Cargar configuración JWT — prioridad: variables de entorno (Render usa __)
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

            options.Events = new JwtBearerEvents
            {
                // SignalR: cuando el transporte es WebSockets (o SSE), el browser
                // no puede enviar el header Authorization. El token llega en el
                // query string ?access_token=... Hay que copiarlo al contexto
                // ANTES de la validación, solo para paths del hub.
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments("/hubs"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                },
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(ApiResponseFactory.Error("No autorizado"));
                },
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(ApiResponseFactory.Error("Acceso denegado"));
                }
            };
        });

        services.AddAuthorization();
    }

    /// <summary>Retorna el primer valor no vacío de la lista.</summary>
    private static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
}
