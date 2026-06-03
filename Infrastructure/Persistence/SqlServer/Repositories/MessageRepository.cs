using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence.Repositories;
using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Shared.Helpers;

namespace Infrastructure.Persistence.SqlServer.Repositories;

/// <summary>
/// Repositorio de base de datos encargado del almacenamiento, control de estados de lectura, eliminación lógica y consultas de los mensajes directos (DMs).
/// </summary>
public class MessageRepository : GenericRepository<Message, Guid>, IMessageRepository
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="MessageRepository"/> con el contexto de base de datos.
    /// </summary>
    /// <param name="context">El contexto de base de datos <see cref="TwitterDbContext"/>.</param>
    public MessageRepository(TwitterDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene de forma asíncrona la lista paginada de mensajes intercambiados en la conversación de dos usuarios específicos, ordenados de más recientes a más antiguos.
    /// </summary>
    /// <param name="user1Id">Identificador único del primer usuario.</param>
    /// <param name="user2Id">Identificador único del segundo usuario.</param>
    /// <param name="limit">Cantidad máxima de mensajes a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>La lista de mensajes <see cref="Message"/> resultantes.</returns>
    public async Task<List<Message>> GetConversationAsync(Guid user1Id, Guid user2Id, int limit, int offset)
    {
        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await _context.Messages
            .Where(m => (m.SenderId == user1Id && m.ReceiverId == user2Id) ||
                        (m.SenderId == user2Id && m.ReceiverId == user1Id))
            .OrderByDescending(m => m.CreatedAt)
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Skip(normalizedOffset)
            .Take(normalizedLimit)
            .ToListAsync();
    }

    /// <summary>
    /// Recupera de forma asíncrona la lista paginada de las conversaciones activas de un usuario, obteniendo únicamente el último mensaje de cada chat.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <param name="limit">Cantidad máxima de conversaciones a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>Una lista conteniendo el último mensaje de cada conversación.</returns>
    public async Task<List<Message>> GetConversationsListAsync(Guid userId, int limit, int offset)
    {
        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        // Obtener IDs únicos de usuarios con los que ha conversado
        var userIds = await _context.Messages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Distinct()
            .ToListAsync();

        // Para cada usuario, obtener el último mensaje
        var lastMessages = new List<Message>();
        foreach (var otherUserId in userIds)
        {
            var lastMessage = await _context.Messages
                .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                            (m.SenderId == otherUserId && m.ReceiverId == userId))
                .OrderByDescending(m => m.CreatedAt)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .FirstOrDefaultAsync();

            if (lastMessage != null)
            {
                lastMessages.Add(lastMessage);
            }
        }

        // Ordenar por fecha y paginar
        return lastMessages
            .OrderByDescending(m => m.CreatedAt)
            .Skip(normalizedOffset)
            .Take(normalizedLimit)
            .ToList();
    }

    /// <summary>
    /// Obtiene de forma asíncrona todos los mensajes marcados como no leídos dirigidos al usuario receptor especificado.
    /// </summary>
    /// <param name="receiverId">Identificador del usuario receptor.</param>
    /// <returns>Una lista conteniendo los mensajes no leídos.</returns>
    public async Task<List<Message>> GetUnreadMessages(Guid receiverId)
    {
        return await _context.Messages
            .Where(m => m.ReceiverId == receiverId && !m.IsRead)
            .OrderByDescending(m => m.CreatedAt)
            .Include(m => m.Sender)
            .ToListAsync();
    }

    /// <summary>
    /// Cuenta de forma asíncrona el total de mensajes no leídos recibidos por el usuario.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <returns>La cantidad total de mensajes no leídos.</returns>
    public async Task<int> CountUnreadAsync(Guid userId)
    {
        return await _context.Messages
            .CountAsync(m => m.ReceiverId == userId && !m.IsRead);
    }

    /// <summary>
    /// Cuenta de forma asíncrona la cantidad de mensajes no leídos recibidos en la conversación con otro usuario específico.
    /// </summary>
    /// <param name="userId">Identificador del usuario destinatario.</param>
    /// <param name="otherUserId">Identificador del usuario emisor.</param>
    /// <returns>La cantidad de mensajes no leídos en esa conversación.</returns>
    public async Task<int> CountUnreadInConversationAsync(Guid userId, Guid otherUserId)
    {
        return await _context.Messages
            .CountAsync(m => m.ReceiverId == userId && 
                            m.SenderId == otherUserId && 
                            !m.IsRead);
    }

    /// <summary>
    /// Marca de forma asíncrona un mensaje individual específico como leído en la base de datos registrando la fecha de lectura.
    /// Devuelve el mensaje marcado, o null si no existe o ya estaba leído.
    /// </summary>
    /// <param name="messageId">Identificador único del mensaje.</param>
    public async Task<Message?> MarkAsReadAsync(Guid messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message != null && !message.IsRead)
        {
            message.IsRead = true;
            message.ReadAt = DateTimeHelper.UtcNow();
            _context.Messages.Update(message);
            return message;
        }
        return null;
    }

    /// <summary>
    /// Marca de forma asíncrona todos los mensajes pendientes de lectura en una conversación con otro usuario como leídos.
    /// </summary>
    /// <param name="userId">Identificador del usuario destinatario.</param>
    /// <param name="otherUserId">Identificador del usuario emisor.</param>
    public async Task MarkConversationAsReadAsync(Guid userId, Guid otherUserId)
    {
        var unreadMessages = await _context.Messages
            .Where(m => m.ReceiverId == userId && 
                       m.SenderId == otherUserId && 
                       !m.IsRead)
            .ToListAsync();

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
            message.ReadAt = DateTimeHelper.UtcNow();
            _context.Messages.Update(message);
        }
    }

    /// <summary>
    /// Realiza de forma asíncrona la eliminación lógica de un mensaje para el emisor o receptor (soft-delete), impidiendo que se visualice para dicho usuario.
    /// </summary>
    /// <param name="messageId">Identificador único del mensaje.</param>
    /// <param name="userId">Identificador del usuario que solicita la eliminación.</param>
    public async Task DeleteForUserAsync(Guid messageId, Guid userId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message != null)
        {
            if (message.SenderId == userId)
            {
                message.DeletedBySender = true;
            }
            else if (message.ReceiverId == userId)
            {
                message.DeletedByReceiver = true;
            }
            
            _context.Messages.Update(message);
        }
    }

    /// <summary>
    /// Registra modificaciones de datos aplicadas a un mensaje directo.
    /// </summary>
    /// <param name="message">La entidad conteniendo los cambios.</param>
    public void Update(Message message)
    {
        _context.Messages.Update(message);
    }
}
