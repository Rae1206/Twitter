using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Interfaces.Services;
using Application.Models.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shared.Constants;
using Shared.Helpers;

namespace Application.Helpers;

/// <summary>
/// Helper estático para generar tokens JWT y refresh tokens.
/// </summary>
public static class TokenHelper
{
    private static readonly Random rnd = new();

    /// <summary>
    /// Crea un token JWT y lo guarda en caché.
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="roles">Lista de roles del usuario</param>
    /// <param name="configuration">Configuración de la aplicación</param>
    /// <param name="cache">Servicio de caché</param>
    /// <returns>Token JWT como string</returns>
    public static string Create(Guid userId, List<string> roles, IConfiguration configuration, ICacheService cache)
    {
        // 1. Obtener configuración del JWT
        var tokenConfiguration = Configuration(configuration);
        
        // 2. Crear credenciales de firma
        var signingCredentials = new SigningCredentials(tokenConfiguration.SecurityKey, SecurityAlgorithms.HmacSha256);

        // 3. Definir claims del token
        var claims = new[]
        {
            new Claim(ClaimsConstants.USER_ID, userId.ToString()),
            new Claim(ClaimTypes.Role, roles[0])
        };

        // 4. Crear el JWT
        var securityToken = new JwtSecurityToken(
            audience: tokenConfiguration.Audience,
            issuer: tokenConfiguration.Issuer,
            expires: tokenConfiguration.Expiration,
            signingCredentials: signingCredentials,
            claims: claims
        );
        
        // 5. Convertir a string
        var token = new JwtSecurityTokenHandler().WriteToken(securityToken);

        // 6. Guardar en cache con expiración igual al token
        var cacheKey = CacheHelper.AuthTokenCreation(token, tokenConfiguration.ExpirationTimeSpan);
        cache.Create(cacheKey.Key, cacheKey.Expiration, token);

        return token;
    }

    /// <summary>
    /// Crea un refresh token y lo guarda en caché.
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="configuration">Configuración de la aplicación</param>
    /// <param name="cacheService">Servicio de caché</param>
    /// <returns>Refresh token como string</returns>
    public static string CreateRefresh(Guid userId, IConfiguration configuration, ICacheService cacheService)
    {
        // 1. Generar token aleatorio largo (100 caracteres)
        var token = Generate.RandomText(100);
        
        // 2. Crear clave de cache
        var cacheKey = CacheHelper.AuthRefreshTokenCreation(token, configuration);

        // 3. Guardar en cache con datos del refresh token
        cacheService.Create(cacheKey.Key, cacheKey.Expiration, new RefreshToken
        {
            UserId = userId,
            ExpirationInDays = cacheKey.Expiration
        });

        return token;
    }

    /// <summary>
    /// Carga la configuración del JWT desde IConfiguration.
    /// Prioriza variables de entorno sobre appsettings.json.
    /// </summary>
    /// <param name="configuration">Configuración de la aplicación</param>
    /// <returns>TokenConfiguration con los datos cargados</returns>
    /// <exception cref="Exception">Lanza si falta alguna propiedad requerida</exception>
    public static TokenConfiguration Configuration(IConfiguration configuration)
    {
        // Cargar configuración desde env vars o appsettings ( Render用__)
        var issuer = Environment.GetEnvironmentVariable("Jwt__Issuer")
            ?? Environment.GetEnvironmentVariable(ConfigurationConstants.JWT_ISSUER)
            ?? configuration[ConfigurationConstants.JWT_ISSUER]
            ?? throw new Exception(ResponseConstants.ConfigurationPropertyNotFound(ConfigurationConstants.JWT_ISSUER));

        var audience = Environment.GetEnvironmentVariable("Jwt__Audience")
            ?? Environment.GetEnvironmentVariable(ConfigurationConstants.JWT_AUDIENCE)
            ?? configuration[ConfigurationConstants.JWT_AUDIENCE]
            ?? throw new Exception(ResponseConstants.ConfigurationPropertyNotFound(ConfigurationConstants.JWT_AUDIENCE));

        var privateKey = Environment.GetEnvironmentVariable("Jwt__PrivateKey")
            ?? Environment.GetEnvironmentVariable(ConfigurationConstants.JWT_PRIVATE_KEY)
            ?? configuration[ConfigurationConstants.JWT_PRIVATE_KEY]
            ?? throw new Exception(ResponseConstants.ConfigurationPropertyNotFound(ConfigurationConstants.JWT_PRIVATE_KEY));

        // Crear clave de seguridad simétrica
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey));

        // Calcular expiración aleatoria entre min y max ( Render用__)
        var now = DateTimeHelper.UtcNow();
        var minExpires = Convert.ToInt32(
            Environment.GetEnvironmentVariable("Jwt__ExpirationInMinutesMin")
            ?? configuration[ConfigurationConstants.JWT_EXPIRATION_IN_MINUTES_MIN] 
            ?? "1");
        var maxExpires = Convert.ToInt32(
            Environment.GetEnvironmentVariable("Jwt__ExpirationInMinutesMax")
            ?? configuration[ConfigurationConstants.JWT_EXPIRATION_IN_MINUTES_MAX] 
            ?? "5");
        var randomExpiration = rnd.Next(minExpires, maxExpires);
        var timespanExpiration = TimeSpan.FromMinutes(randomExpiration);
        var datetimeExpiration = now.Add(TimeSpan.FromMinutes(randomExpiration));

        return new TokenConfiguration
        {
            Issuer = issuer,
            Audience = audience,
            SecurityKey = securityKey,
            Expiration = datetimeExpiration,
            ExpirationTimeSpan = timespanExpiration
        };
    }
}