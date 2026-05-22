using System;
using Twitter.Domain.Database.SqlServer.Entities.Enums;

namespace Twitter.Domain.Database.SqlServer.Entities;

public class PostMedia
{
    public Guid MediaId { get; set; }

    public Guid? PostId { get; set; }

    public Guid UserId { get; set; }

    public MediaType MediaType { get; set; }

    public string FileName { get; set; } = null!;

    public string StoragePath { get; set; } = null!;

    public string Url { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Post? Post { get; set; }
}
