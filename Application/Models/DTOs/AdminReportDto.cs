using System;

namespace Application.Models.DTOs;

/// <summary>
/// Objeto de transferencia de datos (DTO) que representa un reporte en el panel de administración.
/// </summary>
public class AdminReportDto
{
    /// <summary>
    /// Identificador único del reporte.
    /// </summary>
    public Guid ReportId { get; set; }

    /// <summary>
    /// Identificador único del usuario que realizó el reporte.
    /// </summary>
    public Guid ReporterUserId { get; set; }

    /// <summary>
    /// Tipo de entidad reportada (por ejemplo, "Post", "User").
    /// </summary>
    public string EntityType { get; set; } = null!;

    /// <summary>
    /// Identificador único de la entidad específica reportada.
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// Identificador único de la publicación reportada, si aplica.
    /// </summary>
    public Guid? PostId { get; set; }

    /// <summary>
    /// Identificador único del usuario reportado, si aplica.
    /// </summary>
    public Guid? ReportedUserId { get; set; }

    /// <summary>
    /// Categoría o motivo general del reporte (por ejemplo, "Spam", "HateSpeech").
    /// </summary>
    public string Category { get; set; } = null!;
    
    /// <summary>
    /// Mapea a la categoría para que funcione el 'reportTitle' en el frontend (que espera la propiedad 'reason').
    /// </summary>
    public string Reason { get; set; } = null!;
    
    /// <summary>
    /// Descripción detallada o comentarios adicionales agregados por el denunciante.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Estado actual del reporte (por ejemplo, "Pending", "Resolved", "Dismissed").
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Nivel de prioridad asignado al reporte.
    /// </summary>
    public byte Priority { get; set; }

    /// <summary>
    /// Detalle de la resolución o de la acción tomada para resolver el reporte.
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
    /// Fecha y hora (UTC) de creación del reporte.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
