using Application.Interfaces.Repositories;
using Application.Models.DTOs;

namespace Application.Repositories;

public class MessageRepository : IMessageRepository
{
    // TODO: Implement with EF Core

    public MessageDto Create(MessageDto message)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public bool Delete(Guid messageId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public List<MessageDto> GetConversation(Guid userId1, Guid userId2, int limit, int offset)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public MessageDto? GetById(Guid messageId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public int GetUnreadCount(Guid userId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public bool MarkAsRead(Guid messageId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public List<MessageDto> GetRecentConversations(Guid userId, int limit, int offset)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }
}