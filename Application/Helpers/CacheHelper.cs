using Application.Models.Helpers;
using Microsoft.Extensions.Configuration;
using Shared.Constants;

namespace Application.Helpers;

/// <summary>
/// Helper estático para generar claves de caché para autenticación.
/// </summary>
public static class CacheHelper
{
    /// <summary>
    /// Genera la clave para almacenar el token JWT en caché.
    /// Formato: auth:tokens:{token}
    /// </summary>
    public static string AuthTokenKey(string value)
    {
        return $"auth:tokens:{value}";
    }

    /// <summary>
    /// Crea una CacheKey con la clave y expiración para el token JWT.
    /// </summary>
    public static CacheKey AuthTokenCreation(string value, TimeSpan expiration)
    {
        return new CacheKey
        {
            Key = AuthTokenKey(value),
            Expiration = expiration
        };
    }

    /// <summary>
    /// Genera la clave para almacenar el refresh token en caché.
    /// Formato: auth:refresh_tokens:{token}
    /// </summary>
    public static string AuthRefreshTokenKey(string value)
    {
        return $"auth:refresh_tokens:{value}";
    }

    /// <summary>
    /// Crea una CacheKey con la clave y expiración para el refresh token.
    /// </summary>
    public static CacheKey AuthRefreshTokenCreation(string value, IConfiguration configuration)
    {
        return new CacheKey
        {
            Key = AuthRefreshTokenKey(value),
            Expiration = TimeSpan.FromDays(Convert.ToInt32(
                Environment.GetEnvironmentVariable("Auth__RefreshToken__ExpirationInDays")
                ?? configuration[ConfigurationConstants.AUTH_REFRESH_TOKEN_EXPIRATION_IN_DAYS] 
                ?? "15"))
        };
    }
}