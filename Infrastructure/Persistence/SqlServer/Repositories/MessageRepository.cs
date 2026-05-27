using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence.Repositories;
using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Shared.Helpers;

namespace Infrastructure.Persistence.SqlServer.Repositories;

/// <summary>
/// Repositorio de mensajes directos.
/// Hereda GenericRepository para lectura.
/// Usa UnitOfWork para escritura.
/// </summary>
public class MessageRepository : GenericRepository<Message, Guid>, IMessageRepository
{
    public MessageRepository(TwitterDbContext context) : base(context)
    {
    }

    // Obtener conversación entre dos usuarios
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

    // Obtener lista de conversaciones (último mensaje con cada usuario)
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

    // Obtener mensajes no leídos de un usuario
    public async Task<List<Message>> GetUnreadMessages(Guid receiverId)
    {
        return await _context.Messages
            .Where(m => m.ReceiverId == receiverId && !m.IsRead)
            .OrderByDescending(m => m.CreatedAt)
            .Include(m => m.Sender)
            .ToListAsync();
    }

    // Contar mensajes no leídos
    public async Task<int> CountUnreadAsync(Guid userId)
    {
        return await _context.Messages
            .CountAsync(m => m.ReceiverId == userId && !m.IsRead);
    }

    //Contar mensajes no leídos en conversación específica
    public async Task<int> CountUnreadInConversationAsync(Guid userId, Guid otherUserId)
    {
        return await _context.Messages
            .CountAsync(m => m.ReceiverId == userId && 
                            m.SenderId == otherUserId && 
                            !m.IsRead);
    }

    // Marcar mensaje como leído
    public async Task MarkAsReadAsync(Guid messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message != null && !message.IsRead)
        {
            message.IsRead = true;
            message.ReadAt = DateTimeHelper.UtcNow();
            _context.Messages.Update(message);
        }
    }

    // Marcar conversación como leída
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

    // Eliminar mensaje para usuario (soft delete)
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

    // Actualizar mensaje
    public void Update(Message message)
    {
        _context.Messages.Update(message);
    }
}
