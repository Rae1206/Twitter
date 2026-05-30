using System;

namespace Application.Models.DTOs;

/// <summary>
/// Objeto de transferencia de datos (DTO) que representa un reporte estándar de contenido o de usuario en el sistema.
/// </summary>
public class ReportDto
{
    /// <summary>
    /// Identificador único del reporte.
    /// </summary>
    public Guid ReportId { get; set; }

    /// <summary>
    /// Identificador único del usuario que envía el reporte (denunciante).
    /// </summary>
    public Guid ReporterUserId { get; set; }

    /// <summary>
    /// Tipo de entidad reportada (por ejemplo, "Post", "User", "Comment").
    /// </summary>
    public string EntityType { get; set; } = null!;

    /// <summary>
    /// Identificador único de la entidad específica reportada.
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// Categoría o motivo principal del reporte (por ejemplo, "Harassment", "Spam").
    /// </summary>
    public string Category { get; set; } = null!;

    /// <summary>
    /// Explicación detallada o justificación del reporte provista por el usuario.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Estado actual del reporte (por ejemplo, "Pending", "Resolved", "Dismissed").
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Nivel de prioridad asignado para su atención por parte de moderación.
    /// </summary>
    public byte Priority { get; set; }

    /// <summary>
    /// Detalle de la resolución aplicada al reporte por el administrador.
    /// </summary>
    public string? Resolution { get; set; }

    /// <summary>
    /// Fecha y hora (UTC) en la que se resolvió el reporte.
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// Identificador único del administrador que resolvió el reporte.
    /// </summary>
    public Guid? ResolvedByAdminId { get; set; }

    /// <summary>
    /// Fecha y hora (UTC) en la que se creó el reporte.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
