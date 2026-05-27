using System;

namespace Twitter.Domain.Database.SqlServer.Entities;

public class ContentReport
{
    public Guid ReportId { get; set; }

    public Guid ReporterUserId { get; set; }

    /// <summary>
    /// Tipo de entidad reportada: "Post", "User", "Message".
    /// </summary>
    public string EntityType { get; set; } = null!;

    public Guid EntityId { get; set; }

    /// <summary>
    /// Categoría del reporte: "spam", "hate_speech", "harassment",
    /// "misinformation", "nudity", "violence", "copyright", "other".
    /// </summary>
    public string Category { get; set; } = null!;

    public string? Description { get; set; }

    /// <summary>
    /// Estados: "pending", "resolved", "dismissed".
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Prioridad: 1=Alta, 2=Media, 3=Baja.
    /// </summary>
    public byte Priority { get; set; }


    public string? Resolution { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public Guid? ResolvedByAdminId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User Reporter { get; set; } = null!;


    public virtual User? ResolvedByAdmin { get; set; }
}