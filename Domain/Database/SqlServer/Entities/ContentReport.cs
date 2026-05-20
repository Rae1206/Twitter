using System;

namespace Twitter.Domain.Database.SqlServer.Entities;

public class ContentReport
{
    public Guid ReportId { get; set; }

    public Guid ReporterId { get; set; }

    public string TargetType { get; set; } = null!;

    public string TargetId { get; set; } = null!;

    public string Reason { get; set; } = null!;

    public string Status { get; set; } = null!;

    public Guid? AssignedTo { get; set; }

    public string? Resolution { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public virtual User Reporter { get; set; } = null!;
}
