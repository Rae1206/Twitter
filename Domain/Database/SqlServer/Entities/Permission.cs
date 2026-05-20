using System;

namespace Twitter.Domain.Database.SqlServer.Entities;

public class Permission
{
    public Guid PermissionId { get; set; }

    public string Name { get; set; } = null!;

    public string Module { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
