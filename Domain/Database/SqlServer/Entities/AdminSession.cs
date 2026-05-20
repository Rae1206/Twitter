using System;

namespace Twitter.Domain.Database.SqlServer.Entities;

public class AdminSession
{
    public Guid SessionId { get; set; }

    public Guid AdminUserId { get; set; }

    public DateTime LoginAt { get; set; }

    public DateTime? LogoutAt { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public virtual User AdminUser { get; set; } = null!;
}
