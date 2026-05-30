using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WebApi.Hubs;

/// <summary>
/// Hub de SignalR para mensajería en tiempo real.
/// Eventos emitidos al cliente:
///   - "ReceiveMessage" (MessageDto)        → cuando llega un mensaje nuevo
///   - "UserOnline"     ({UserId,Nickname}) → SOLO cuando el userId pasa de 0 → 1 conexión
///   - "UserOffline"    (string userId)     → SOLO cuando el userId pasa de 1 → 0 conexiones
///   - "UserTyping"     (string senderId)   → typing indicator
///   - "UserStopTyping" (string senderId)   → typing stop
///
/// Métodos invocables por el cliente:
///   - NotifyTyping(receiverId)       → fan-out a "user-{receiverId}"
///   - NotifyStopTyping(receiverId)   → fan-out a "user-{receiverId}"
///   - GetOnlineUsers()               → snapshot inicial de presencia (excluye al solicitante)
/// </summary>
[Authorize]
public class MessageHub : Hub
{
    private readonly IUserService _userService;

    public MessageHub(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Mapa userId → conjunto de ConnectionIds activos.
    /// Un mismo userId puede tener N conexiones (multi-pestaña / multi-dispositivo).
    /// La presencia se considera "online" mientras la cardinalidad sea > 0.
    /// </summary>
    private static readonly Dictionary<string, HashSet<string>> _userConnections = new();
    private static readonly object _connectionsLock = new();

    /// <summary>Registra la conexión del usuario y emite UserOnline si es la primera.</summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            // Inscribir esta conexión en el grupo del usuario para fan-out dirigido.
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

            // Determinar atómicamente si esta es la PRIMERA conexión del userId.
            // Solo en ese caso debe emitirse UserOnline (idempotencia de presencia).
            bool isFirstConnection;
            lock (_connectionsLock)
            {
                if (!_userConnections.TryGetValue(userId, out var connections))
                {
                    connections = new HashSet<string>();
                    _userConnections[userId] = connections;
                }
                isFirstConnection = connections.Count == 0;
                connections.Add(Context.ConnectionId);
            }

            if (isFirstConnection)
            {
                // Resolver nickname una sola vez, fuera del lock, antes de emitir.
                string nickname = "Usuario";
                try
                {
                    var user = await _userService.Get(Guid.Parse(userId));
                    if (user is not null && !string.IsNullOrWhiteSpace(user.Nickname))
                    {
                        nickname = user.Nickname;
                    }
                }
                catch
                {
                    // Si no se puede resolver el usuario, mantenemos el fallback.
                }

                var userInfo = new { UserId = userId, Nickname = nickname };
                await Clients.Others.SendAsync("UserOnline", userInfo);
            }
        }

        await base.OnConnectedAsync();
    }

    /// <summary>Elimina la conexión del usuario y emite UserOffline si era la última.</summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");

            // Determinar atómicamente si esta era la ÚLTIMA conexión del userId.
            bool wasLastConnection = false;
            lock (_connectionsLock)
            {
                if (_userConnections.TryGetValue(userId, out var connections))
                {
                    connections.Remove(Context.ConnectionId);
                    if (connections.Count == 0)
                    {
                        _userConnections.Remove(userId);
                        wasLastConnection = true;
                    }
                }
            }

            if (wasLastConnection)
            {
                await Clients.Others.SendAsync("UserOffline", userId);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Notifica al receptor que el remitente está escribiendo.
    /// </summary>
    public async Task NotifyTyping(string receiverId)
    {
        var senderId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(senderId) && !string.IsNullOrEmpty(receiverId))
        {
            await Clients.Group($"user-{receiverId}").SendAsync("UserTyping", senderId);
        }
    }

    /// <summary>
    /// Notifica al receptor que el remitente dejó de escribir.
    /// </summary>
    public async Task NotifyStopTyping(string receiverId)
    {
        var senderId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(senderId) && !string.IsNullOrEmpty(receiverId))
        {
            await Clients.Group($"user-{receiverId}").SendAsync("UserStopTyping", senderId);
        }
    }

    /// <summary>
    /// Snapshot inicial de presencia: lista de userIds actualmente conectados,
    /// EXCLUYENDO al solicitante (no tiene sentido aparecer "online" en tu propia lista).
    /// El cliente lo invoca una vez al conectarse para sembrar su estado local
    /// y luego se mantiene actualizado por los eventos UserOnline / UserOffline.
    /// </summary>
    public List<OnlineUserInfo> GetOnlineUsers()
    {
        var requesterId = Context.UserIdentifier;
        lock (_connectionsLock)
        {
            return _userConnections.Keys
                .Where(id => id != requesterId)
                .Select(id => new OnlineUserInfo { UserId = id })
                .ToList();
        }
    }

    /// <summary>
    /// Helper sincrónico (uso interno / debugging).
    /// </summary>
    public bool IsUserOnline(string userId)
    {
        lock (_connectionsLock)
        {
            return _userConnections.TryGetValue(userId, out var connections) && connections.Count > 0;
        }
    }
}

/// <summary>
/// Forma del item devuelto por GetOnlineUsers.
/// Se mantiene como objeto (no string plano) para poder enriquecer con nickname/avatar
/// en el futuro sin romper el contrato del cliente.
/// </summary>
public class OnlineUserInfo
{
    public string UserId { get; set; } = string.Empty;
}
