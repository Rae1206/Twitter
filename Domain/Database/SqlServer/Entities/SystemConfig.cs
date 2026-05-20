using System;

namespace Twitter.Domain.Database.SqlServer.Entities;

public class SystemConfig
{
    public Guid ConfigId { get; set; }

    public string Key { get; set; } = null!;

    public string Value { get; set; } = null!;

    public string ValueType { get; set; } = "string";

    public string? Module { get; set; }

    public string? Description { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedByUserId { get; set; }
}
