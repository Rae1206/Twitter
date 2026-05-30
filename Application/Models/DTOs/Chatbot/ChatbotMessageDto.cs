namespace Application.Models.DTOs.Chatbot;

/// <summary>
/// Objeto de transferencia de datos (DTO) que representa un mensaje individual en una conversación con el chatbot.
/// </summary>
public class ChatbotMessageDto
{
    /// <summary>
    /// Identificador único del mensaje del chatbot.
    /// </summary>
    public Guid ChatbotMessageId { get; set; }

    /// <summary>
    /// Identificador único del usuario asociado con el mensaje.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Rol del emisor del mensaje (por ejemplo, "user" o "assistant").
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Contenido de texto del mensaje.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Modelo de Inteligencia Artificial utilizado para generar el mensaje (si aplica).
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Fecha y hora (UTC) en la que se creó el mensaje.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
