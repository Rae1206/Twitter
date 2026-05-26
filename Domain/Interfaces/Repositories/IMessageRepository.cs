using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz del repositorio de mensajes.
/// Hereda de IGenericRepository para operaciones CRUD genéricas.
/// </summary>
public interface IMessageRepository : IGenericRepository<Message, Guid>
{
    Task<List<Message>> GetConversation(Guid userId1, Guid userId2, int limit = 0, int offset = 0);
    Task<List<Message>> GetUnreadMessages(Guid receiverId);
    Task<int> GetUnreadCount(Guid receiverId);
    Task MarkAsRead(Guid messageId);
    Task MarkConversationAsRead(Guid senderId, Guid receiverId);
}