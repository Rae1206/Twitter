namespace Application.Models.DTOs.Chatbot;

public class ChatbotReplyDto
{
    public ChatbotMessageDto UserMessage { get; set; } = new();
    public ChatbotMessageDto AssistantMessage { get; set; } = new();
    public string Model { get; set; } = string.Empty;
}
