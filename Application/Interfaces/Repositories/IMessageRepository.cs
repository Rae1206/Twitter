using Application.Models.DTOs;

namespace Application.Interfaces.Repositories;

public interface IMessageRepository
{
    MessageDto Create(MessageDto message);
    bool Delete(Guid messageId);
    MessageDto? GetById(Guid messageId);
    List<MessageDto> GetConversation(Guid userId1, Guid userId2, int limit, int offset);
    List<MessageDto> GetRecentConversations(Guid userId, int limit, int offset);
    bool MarkAsRead(Guid messageId);
    int GetUnreadCount(Guid userId);
}