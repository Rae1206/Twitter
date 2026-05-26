using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twitter.Domain.Database.SqlServer.Entities;

namespace Application.Interfaces.Services;

/// <summary>
/// Interfaz del servicio de mensajes.
/// </summary>
public interface IMessageService
{
    Task<Message> SendMessage(Guid senderId, Guid receiverId, string content);
    Task<List<Message>> GetConversation(Guid userId1, Guid userId2, int limit = 0, int offset = 0);
    Task<List<Message>> GetUnreadMessages(Guid receiverId);
    Task<int> GetUnreadCount(Guid receiverId);
    Task MarkAsRead(Guid messageId);
    Task MarkConversationAsRead(Guid senderId, Guid receiverId);
}