using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;

namespace Infrastructure.Persistence.SqlServer.Repositories;

public class ChatbotMessageRepository(TwitterDbContext context)
    : GenericRepository<ChatbotMessage, Guid>(context), IChatbotMessageRepository
{
    public async Task<List<ChatbotMessage>> GetHistoryAsync(
        Guid userId,
        int limit,
        int offset,
        CancellationToken cancellationToken = default)
    {
        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0
            ? Shared.Constants.ChatbotConstants.DefaultHistoryLimit
            : Math.Min(limit, Shared.Constants.ChatbotConstants.MaxHistoryLimit);

        return await _context.ChatbotMessages
            .Where(message => message.UserId == userId)
            .OrderBy(message => message.CreatedAt)
            .Skip(normalizedOffset)
            .Take(normalizedLimit)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ChatbotMessage>> GetRecentConversationAsync(
        Guid userId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = limit <= 0 ? Shared.Constants.ChatbotConstants.RecentContextMessageLimit : limit;

        var recentMessages = await _context.ChatbotMessages
            .Where(message => message.UserId == userId)
            .OrderByDescending(message => message.CreatedAt)
            .Take(normalizedLimit)
            .ToListAsync(cancellationToken);

        return recentMessages
            .OrderBy(message => message.CreatedAt)
            .ToList();
    }
}
