using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Shared.Configuration;
using Shared.Helpers;

namespace WebApi.Extensions;

public static class AuthExtensions
{
    /// <summary>
    /// Configura la autenticación JWT usando TokenConfiguration desde IConfiguration.
    /// </summary>
    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Cargar configuración desde IConfiguration
        var tokenConfig = TokenHelper.LoadFromConfiguration(configuration);

        // Registrar como servicio inyectable (patrón Options)
        services.Configure<TokenConfiguration>(
            configuration.GetSection(TokenConfiguration.SectionName));

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
                ValidIssuer = tokenConfig.Issuer,
                ValidAudience = tokenConfig.Audience,
                IssuerSigningKey = tokenConfig.GetSecurityKey(),
                ClockSkew = TimeSpan.Zero,
                
                // Validar roles desde el token
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.Name
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    // Los roles ya vienen en el token como ClaimTypes.Role
                    // No necesitamos hacer nada extra porque TokenHelper los agrega correctamente
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();
    }
}
