namespace Application.Models.DTOs.Chatbot;

/// <summary>
/// Objeto de transferencia de datos (DTO) que representa una respuesta del chatbot, vinculando el mensaje del usuario con la respuesta del asistente.
/// </summary>
public class ChatbotReplyDto
{
    /// <summary>
    /// Información del mensaje enviado por el usuario.
    /// </summary>
    public ChatbotMessageDto UserMessage { get; set; } = new();

    /// <summary>
    /// Información del mensaje generado por el asistente (chatbot).
    /// </summary>
    public ChatbotMessageDto AssistantMessage { get; set; } = new();

    /// <summary>
    /// Nombre del modelo de IA que generó la respuesta.
    /// </summary>
    public string Model { get; set; } = string.Empty;
}
