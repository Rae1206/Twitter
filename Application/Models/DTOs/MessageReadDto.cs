using System;

namespace Application.Models.DTOs;

/// <summary>
/// DTO para notificar en tiempo real que uno o más mensajes fueron leídos.
/// Se envía al emisor original vía SignalR.
/// </summary>
public class MessageReadDto
{
    /// <summary>
    /// Identificador único del mensaje que fue leído.
    /// Null cuando se marcó toda una conversación como leída.
    /// </summary>
    public Guid? MessageId { get; set; }

    /// <summary>
    /// Identificador del usuario que leyó el mensaje (receptor).
    /// </summary>
    public Guid ReadBy { get; set; }

    /// <summary>
    /// Identificador del usuario que envió el mensaje (emisor).
    /// </summary>
    public Guid SenderId { get; set; }

    /// <summary>
    /// Fecha y hora (UTC) en la que se marcó como leído.
    /// </summary>
    public DateTime ReadAt { get; set; }
}