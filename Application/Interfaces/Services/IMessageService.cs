using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Models.DTOs;

namespace Application.Interfaces.Services;

/// <summary>
/// Interfaz del servicio de mensajes.
/// </summary>
public interface IMessageService
{
    Task<MessageDto> SendMessage(Guid senderId, Guid receiverId, string content);
    Task<List<MessageDto>> GetConversation(Guid userId1, Guid userId2, int limit = 0, int offset = 0);
    Task<List<MessageDto>> GetConversationsList(Guid userId, int limit = 0, int offset = 0);
    Task<List<MessageDto>> GetUnreadMessages(Guid receiverId);
    Task<int> GetUnreadCount(Guid receiverId);
    Task<int> GetUnreadCountInConversation(Guid userId, Guid otherUserId);
    Task MarkAsRead(Guid messageId, Guid userId);
    Task MarkConversationAsRead(Guid userId, Guid otherUserId);
    Task DeleteMessage(Guid messageId, Guid userId);
}