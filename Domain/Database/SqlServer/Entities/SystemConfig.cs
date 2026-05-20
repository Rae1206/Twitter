using System;

namespace Twitter.Domain.Database.SqlServer.Entities;

public class SystemConfig
{
    public Guid ConfigId { get; set; }

    public string Key { get; set; } = null!;

    public string Value { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsEditable { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
