using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Shared.Helpers;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de mensajes.
/// </summary>
public class MessageRepository : GenericRepository<Message, Guid>, IMessageRepository
{
    public MessageRepository(TwitterDbContext context) : base(context)
    {
    }

    public async Task<List<Message>> GetConversation(Guid userId1, Guid userId2, int limit = 0, int offset = 0)
    {
        var query = _context.Messages
            .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                        (m.SenderId == userId2 && m.ReceiverId == userId1))
            .OrderByDescending(m => m.CreatedAt);

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await query.Skip(normalizedOffset).Take(normalizedLimit).ToListAsync();
    }

    public async Task<List<Message>> GetUnreadMessages(Guid receiverId)
    {
        return await _context.Messages
            .Where(m => m.ReceiverId == receiverId && !m.IsRead)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCount(Guid receiverId)
    {
        return await _context.Messages.CountAsync(m => m.ReceiverId == receiverId && !m.IsRead);
    }

    public async Task MarkAsRead(Guid messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message is not null)
        {
            message.IsRead = true;
            message.ReadAt = DateTimeHelper.UtcNow();
        }
    }

    public async Task MarkConversationAsRead(Guid senderId, Guid receiverId)
    {
        var unread = await _context.Messages
            .Where(m => m.SenderId == senderId && m.ReceiverId == receiverId && !m.IsRead)
            .ToListAsync();

        var now = DateTimeHelper.UtcNow();
        foreach (var message in unread)
        {
            message.IsRead = true;
            message.ReadAt = now;
        }
    }
}