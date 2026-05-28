using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace WebApi.Hubs;

/// <summary>
/// Proveedor personalizado de UserId para SignalR
/// Extrae el userId del claim "UserId" del token JWT
/// </summary>
public class UserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        // Intentar obtener el userId del claim "UserId" (nuestro claim personalizado)
        var userId = connection.User?.FindFirst("UserId")?.Value
                     ?? connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? connection.User?.FindFirst("sub")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            Console.WriteLine($"SignalR: UserId obtenido del token: {userId}");
        }
        else
        {
            Console.WriteLine("SignalR: No se pudo obtener el UserId del token");
        }

        return userId;
    }
}
