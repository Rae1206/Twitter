using System.Security.Claims;
using Shared.Constants;

namespace WebApi.Extensions;

/// <summary>
/// Extensiones para extraer el UserId desde los claims del usuario.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>Intenta obtener el UserId del usuario. Retorna null si no existe.</summary>
    public static Guid? TryGetUserId(this ClaimsPrincipal? user)
    {
        var claim = user?.FindFirst(ClaimsConstants.USER_ID)?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : null;
    }

    /// <summary>Obtiene el UserId o lanza excepción si no está autenticado.</summary>
    public static Guid GetRequiredUserId(this ClaimsPrincipal? user)
    {
        return user.TryGetUserId()
            ?? throw new UnauthorizedAccessException(ResponseConstants.USER_NOT_EXISTS);
    }
}
