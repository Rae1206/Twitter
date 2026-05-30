namespace Application.Models.DTOs.Chatbot;

public class ChatbotMessageDto
{
    public Guid ChatbotMessageId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Model { get; set; }
    public DateTime CreatedAt { get; set; }
}
