namespace Application.Models.Helpers;

/// <summary>
/// Representa una clave de caché con tiempo de expiración asociado.
/// Es un record inmutable que encapsula la clave y su configuración de expiración.
/// </summary>
public record CacheKey(string Key, TimeSpan Expiration)
{
    /// <summary>
    /// Prefijo para tokens de autenticación.
    /// </summary>
    public const string AuthTokenPrefix = "AuthToken";

    /// <summary>
    /// Prefijo para tokens de refresco.
    /// </summary>
    public const string RefreshTokenPrefix = "RefreshToken";

    /// <summary>
    /// Prefijo para datos de usuario.
    /// </summary>
    public const string UserPrefix = "User";

    /// <summary>
    /// Prefijo para datos de posts.
    /// </summary>
    public const string PostPrefix = "Post";

    /// <summary>
    /// Prefijo para listas paginadas.
    /// </summary>
    public const string ListPrefix = "List";
}
