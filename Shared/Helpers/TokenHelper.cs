using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shared.Configuration;

namespace Shared.Helpers;

/// <summary>
/// Helper estático para generar tokens JWT usando TokenConfiguration.
/// </summary>
public static class TokenHelper
{
    /// <summary>
    /// Genera un token JWT usando la configuración proporcionada.
    /// </summary>
    public static string GenerateJwtToken(
        Guid userId,
        string fullName,
        string role,
        TokenConfiguration config)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, fullName),
            new Claim("role", role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: config.Issuer,
            audience: config.Audience,
            claims: claims,
            expires: config.GetExpirationDate(),
            signingCredentials: new SigningCredentials(
                config.GetSecurityKey(),
                SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Carga la configuración de tokens desde IConfiguration.
    /// Busca en la sección "Jwt".
    /// </summary>
    public static TokenConfiguration LoadFromConfiguration(IConfiguration configuration)
    {
        var config = new TokenConfiguration();
        configuration.GetSection(TokenConfiguration.SectionName).Bind(config);

        if (string.IsNullOrEmpty(config.SecretKey))
        {
            throw new InvalidOperationException(
                "El JWT SecretKey no está configurado en la sección 'Jwt:SecretKey'");
        }

        return config;
    }
}
