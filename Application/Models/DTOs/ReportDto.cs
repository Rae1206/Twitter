using System;

namespace Application.Models.DTOs;

public class ReportDto
{
    public Guid ReportId { get; set; }
    public Guid ReporterUserId { get; set; }
    public string EntityType { get; set; } = null!;
    public Guid EntityId { get; set; }
    public string Category { get; set; } = null!;
    public string? Description { get; set; }
    public string Status { get; set; } = null!;
    public byte Priority { get; set; }
    public string? Resolution { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedByAdminId { get; set; }
    public DateTime CreatedAt { get; set; }
}
