using Microsoft.AspNetCore.SignalR;

namespace WebApi.Hubs;

/// <summary>
/// Hub de SignalR para mensajería en tiempo real.
/// </summary>
public class MessageHub : Hub
{
    // Diccionario para rastrear usuarios conectados
    private static readonly Dictionary<string, HashSet<string>> _userConnections = new();

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            // Agregar conexión al grupo del usuario
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

            // Rastrear conexión
            lock (_userConnections)
            {
                if (!_userConnections.ContainsKey(userId))
                {
                    _userConnections[userId] = new HashSet<string>();
                }
                _userConnections[userId].Add(Context.ConnectionId);
            }

            // Notificar a todos que el usuario está en línea
            await Clients.Others.SendAsync("UserOnline", userId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            // Remover del grupo
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");

            // Remover conexión del rastreo
            bool isLastConnection = false;
            lock (_userConnections)
            {
                if (_userConnections.ContainsKey(userId))
                {
                    _userConnections[userId].Remove(Context.ConnectionId);
                    if (_userConnections[userId].Count == 0)
                    {
                        _userConnections.Remove(userId);
                        isLastConnection = true;
                    }
                }
            }

            // Si era la última conexión, notificar que está offline
            if (isLastConnection)
            {
                await Clients.Others.SendAsync("UserOffline", userId);
            }
        }
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Notifica al receptor que el usuario está escribiendo
    /// </summary>
    public async Task NotifyTyping(string receiverId)
    {
        var senderId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(senderId))
        {
            await Clients.Group($"user-{receiverId}").SendAsync("UserTyping", senderId);
        }
    }

    /// <summary>
    /// Notifica al receptor que el usuario dejó de escribir
    /// </summary>
    public async Task NotifyStopTyping(string receiverId)
    {
        var senderId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(senderId))
        {
            await Clients.Group($"user-{receiverId}").SendAsync("UserStopTyping", senderId);
        }
    }

    /// <summary>
    /// Verifica si un usuario está en línea
    /// </summary>
    public bool IsUserOnline(string userId)
    {
        lock (_userConnections)
        {
            return _userConnections.ContainsKey(userId) && _userConnections[userId].Count > 0;
        }
    }
}