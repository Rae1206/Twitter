using Application.Models.DTOs;
using Application.Models.Requests.User;
using Application.Models.Responses;

namespace Application.Interfaces.Services;

/// <summary>
/// Interfaz del servicio de autenticación.
/// Gestiona login/logout y tokens JWT con rotación automática cada 1-5 min.
/// La sesión total dura 24 horas desde el login.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Inicia sesión con las credenciales proporcionadas.
    /// Crea una sesión de 24 horas con token inicial.
    /// </summary>
    LoginResponse Login(LoginUserRequest model);

    /// <summary>
    /// Obtiene el token actual o lo rota automáticamente si expiró (cada 1-5 min).
    /// Si la sesión de 24 horas expiró, lanza excepción.
    /// </summary>
    /// <returns>TokenResult con el token y si fue rotado</returns>
    TokenResult GetOrRotateToken(Guid userId);

    /// <summary>
    /// Obtiene el token en caché sin rotar (para lectura pura).
    /// </summary>
    string? GetCachedToken(Guid userId);

    /// <summary>
    /// Invalida la sesión completamente (logout).
    /// </summary>
    bool InvalidateTokenCache(Guid userId);
}
