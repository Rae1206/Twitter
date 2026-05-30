using System;

namespace Application.Models.DTOs;

/// <summary>
/// Objeto de transferencia de datos (DTO) que representa un mensaje directo entre dos usuarios.
/// </summary>
public class MessageDto
{
    /// <summary>
    /// Identificador único del mensaje.
    /// </summary>
    public Guid MessageId { get; set; }

    /// <summary>
    /// Identificador único del usuario remitente.
    /// </summary>
    public Guid SenderId { get; set; }

    /// <summary>
    /// Identificador único del usuario destinatario.
    /// </summary>
    public Guid ReceiverId { get; set; }

    /// <summary>
    /// Nombre de usuario (username) del remitente.
    /// </summary>
    public string SenderUsername { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de usuario (username) del destinatario.
    /// </summary>
    public string ReceiverUsername { get; set; } = string.Empty;

    /// <summary>
    /// URL o ruta del avatar del remitente.
    /// </summary>
    public string? SenderAvatar { get; set; }

    /// <summary>
    /// URL o ruta del avatar del destinatario.
    /// </summary>
    public string? ReceiverAvatar { get; set; }

    /// <summary>
    /// Contenido de texto del mensaje.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el mensaje ha sido leído por el destinatario.
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// Fecha y hora (UTC) en la que se envió el mensaje.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
