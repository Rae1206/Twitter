using System.ComponentModel.DataAnnotations;
using Shared.Constants;

namespace Application.Models.Requests.Chatbot;

public class SendChatbotMessageRequest
{
    [Required(ErrorMessage = ValidationConstants.REQUIRED)]
    [MinLength(1, ErrorMessage = ValidationConstants.MIN_LENGTH)]
    [MaxLength(ChatbotConstants.MaxMessageLength, ErrorMessage = ValidationConstants.MAX_LENGTH)]
    public string Message { get; set; } = string.Empty;
}
