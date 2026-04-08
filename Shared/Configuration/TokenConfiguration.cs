using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Shared.Configuration;

/// <summary>
/// Configuración de tokens JWT. Se configura desde IConfiguration usando el patrón Options.
/// Sección: "Jwt" en appsettings.json
/// </summary>
public class TokenConfiguration
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "Twitter.WebApi";
    public string Audience { get; set; } = "Twitter.WebApi.Users";
    public int ExpirationMinutes { get; set; } = 1440;

    // Rango de minutos para expiración aleatoria del caché del token
    public int CacheMinMinutes { get; set; } = 1;
    public int CacheMaxMinutes { get; set; } = 5;

    private static readonly Random RandomGenerator = new();

    /// <summary>
    /// Genera la SecurityKey a partir del SecretKey.
    /// </summary>
    public SymmetricSecurityKey GetSecurityKey()
    {
        if (string.IsNullOrEmpty(SecretKey))
        {
            throw new InvalidOperationException("El JWT SecretKey no está configurado");
        }

        var key = Encoding.UTF8.GetBytes(SecretKey);
        return new SymmetricSecurityKey(key);
    }

    /// <summary>
    /// Fecha de expiración del token JWT calculada a partir del momento actual.
    /// </summary>
    public DateTime GetExpirationDate()
    {
        return DateTime.UtcNow.AddMinutes(ExpirationMinutes);
    }

    /// <summary>
    /// Genera un tiempo de expiración aleatorio para el caché del token (1-5 minutos por defecto).
    /// Esto evita ataques de timing donde todos los tokens expiran al mismo tiempo.
    /// </summary>
    public TimeSpan GetRandomCacheExpiration()
    {
        var minutes = RandomGenerator.Next(CacheMinMinutes, CacheMaxMinutes + 1);
        return TimeSpan.FromMinutes(minutes);
    }
}
