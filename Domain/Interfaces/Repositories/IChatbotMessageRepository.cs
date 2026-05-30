using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

public interface IChatbotMessageRepository : IGenericRepository<ChatbotMessage, Guid>
{
    Task<List<ChatbotMessage>> GetHistoryAsync(Guid userId, int limit, int offset, CancellationToken cancellationToken = default);

    Task<List<ChatbotMessage>> GetRecentConversationAsync(Guid userId, int limit, CancellationToken cancellationToken = default);
}
