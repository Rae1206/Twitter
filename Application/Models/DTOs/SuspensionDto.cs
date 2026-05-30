using System;

namespace Application.Models.DTOs;

/// <summary>
/// Objeto de transferencia de datos (DTO) que representa la suspensión temporal o permanente de un usuario.
/// </summary>
public class SuspensionDto
{
    /// <summary>
    /// Identificador único del registro de suspensión.
    /// </summary>
    public Guid SuspensionId { get; set; }

    /// <summary>
    /// Identificador único del usuario suspendido.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Identificador único del administrador que aplicó la suspensión.
    /// </summary>
    public Guid AdminUserId { get; set; }

    /// <summary>
    /// Tipo de suspensión aplicada (por ejemplo, "Temporal", "Permanent").
    /// </summary>
    public string SuspensionType { get; set; } = null!;

    /// <summary>
    /// Razón o motivo que justifica la suspensión del usuario.
    /// </summary>
    public string Reason { get; set; } = null!;

    /// <summary>
    /// Fecha y hora (UTC) de finalización de la suspensión (si es temporal; null si es permanente).
    /// </summary>
    public DateTime? EndsAt { get; set; }

    /// <summary>
    /// Indica si la suspensión está actualmente activa.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Identificador único del usuario o administrador que levantó la suspensión antes de tiempo.
    /// </summary>
    public Guid? LiftedByUserId { get; set; }

    /// <summary>
    /// Fecha y hora (UTC) en la que se levantó la suspensión.
    /// </summary>
    public DateTime? LiftedAt { get; set; }

    /// <summary>
    /// Fecha y hora (UTC) de creación del registro de suspensión.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}