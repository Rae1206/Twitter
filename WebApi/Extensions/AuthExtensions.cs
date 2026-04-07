using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Shared.Constants;

namespace WebApi.Extensions;

public static class AuthExtensions
{
    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var secretKey = configuration["Jwt:SecretKey"];

        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("La clave secreta JWT no está configurada");
        }

        var key = Encoding.UTF8.GetBytes(secretKey);

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
                ValidIssuer = JwtConstants.Issuer,
                ValidAudience = JwtConstants.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
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
