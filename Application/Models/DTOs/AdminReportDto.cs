using System;

namespace Application.Models.DTOs;

public class AdminReportDto
{
    public Guid ReportId { get; set; }
    public Guid ReporterUserId { get; set; }
    public string EntityType { get; set; } = null!;
    public Guid EntityId { get; set; }
    public Guid? PostId { get; set; }
    public Guid? ReportedUserId { get; set; }
    public string Category { get; set; } = null!;
    
    /// <summary>
    /// Maps to Category so that frontend's reportTitle works (expects reason property).
    /// </summary>
    public string Reason { get; set; } = null!;
    
    public string? Description { get; set; }
    public string Status { get; set; } = null!;
    public byte Priority { get; set; }
    public string? Resolution { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedByAdminId { get; set; }
    public DateTime CreatedAt { get; set; }
}
