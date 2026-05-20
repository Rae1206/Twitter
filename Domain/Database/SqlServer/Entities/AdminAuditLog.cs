using System;

namespace Twitter.Domain.Database.SqlServer.Entities;

public class AdminAuditLog
{
    public Guid AuditLogId { get; set; }

    public Guid AdminUserId { get; set; }

    public string Action { get; set; } = null!;

    public string EntityType { get; set; } = null!;

    public string? EntityId { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? Reason { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User AdminUser { get; set; } = null!;
}
