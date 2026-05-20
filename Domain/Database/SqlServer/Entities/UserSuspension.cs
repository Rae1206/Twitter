using System;

namespace Twitter.Domain.Database.SqlServer.Entities;

public class UserSuspension
{
    public Guid SuspensionId { get; set; }

    public Guid UserId { get; set; }

    public Guid AdminUserId { get; set; }

    public string SuspensionType { get; set; } = null!;

    public string Reason { get; set; } = null!;

    public DateTime? EndsAt { get; set; }

    public bool IsActive { get; set; }

    public Guid? LiftedByUserId { get; set; }

    public DateTime? LiftedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual User AdminUser { get; set; } = null!;
}
