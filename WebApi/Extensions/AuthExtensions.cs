using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Shared.Constants;

namespace WebApi.Extensions;

/// <summary>
/// Extensiones para configurar la autenticación JWT.
/// </summary>
public static class AuthExtensions
{
    /// <summary>
    /// Configura la autenticación JWT usando la configuración de secret.json.
    /// </summary>
    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Cargar configuración JWT -优先环境变量 ( Render用__)
        var issuer = Environment.GetEnvironmentVariable("Jwt__Issuer")
            ?? Environment.GetEnvironmentVariable(ConfigurationConstants.JWT_ISSUER)
            ?? configuration[ConfigurationConstants.JWT_ISSUER]
            ?? throw new InvalidOperationException($"JWT Issuer no configurado");

        var audience = Environment.GetEnvironmentVariable("Jwt__Audience")
            ?? Environment.GetEnvironmentVariable(ConfigurationConstants.JWT_AUDIENCE)
            ?? configuration[ConfigurationConstants.JWT_AUDIENCE]
            ?? throw new InvalidOperationException($"JWT Audience no configurado");

        var privateKey = Environment.GetEnvironmentVariable("Jwt__PrivateKey")
            ?? Environment.GetEnvironmentVariable(ConfigurationConstants.JWT_PRIVATE_KEY)
            ?? configuration[ConfigurationConstants.JWT_PRIVATE_KEY]
            ?? throw new InvalidOperationException($"JWT PrivateKey no configurado");

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