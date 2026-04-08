using Application.Models.Helpers;

namespace Application.Helpers;

/// <summary>
/// Helper estático para generar claves de caché tipadas y con expiración.
/// </summary>
public static class CacheHelper
{
    /// <summary>
    /// Genera una clave de caché para tokens de autenticación.
    /// Expiración: 5 minutos.
    /// </summary>
    public static CacheKey AuthToken(string token)
    {
        return new CacheKey($"{CacheKey.AuthTokenPrefix}:{token}", TimeSpan.FromMinutes(5));
    }

    /// <summary>
    /// Genera una clave de caché para tokens de refresco.
    /// Expiración: 30 días.
    /// </summary>
    public static CacheKey RefreshToken(string token)
    {
        return new CacheKey($"{CacheKey.RefreshTokenPrefix}:{token}", TimeSpan.FromDays(30));
    }

    /// <summary>
    /// Genera una clave de caché para datos de usuario.
    /// Expiración: 10 minutos.
    /// </summary>
    public static CacheKey User(Guid userId)
    {
        return new CacheKey($"{CacheKey.UserPrefix}:{userId}", TimeSpan.FromMinutes(10));
    }

    /// <summary>
    /// Genera una clave de caché para datos de un post.
    /// Expiración: 5 minutos.
    /// </summary>
    public static CacheKey Post(Guid postId)
    {
        return new CacheKey($"{CacheKey.PostPrefix}:{postId}", TimeSpan.FromMinutes(5));
    }

    /// <summary>
    /// Genera una clave de caché para listas paginadas.
    /// Expiración: 2 minutos.
    /// </summary>
    public static CacheKey List(string listName, int page, int pageSize)
    {
        return new CacheKey(
            $"{CacheKey.ListPrefix}:{listName}:p{page}:s{pageSize}", 
            TimeSpan.FromMinutes(2));
    }
}
