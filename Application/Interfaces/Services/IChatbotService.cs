using Application.Models.DTOs.Chatbot;
using Application.Models.Requests.Chatbot;

namespace Application.Interfaces.Services;

public interface IChatbotService
{
    Task<ChatbotReplyDto> SendMessageAsync(
        Guid currentUserId,
        SendChatbotMessageRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChatbotMessageDto>> GetHistoryAsync(
        Guid currentUserId,
        int limit,
        int offset,
        CancellationToken cancellationToken = default);
}
