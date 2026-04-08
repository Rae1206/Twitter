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
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var claimsIdentity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                    var roleClaim = context.Principal?.FindFirst("role");

                    if (roleClaim != null && claimsIdentity != null)
                    {
                        claimsIdentity.AddClaim(new System.Security.Claims.Claim(
                            System.Security.Claims.ClaimTypes.Role, roleClaim.Value));
                    }

                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();
    }
}
