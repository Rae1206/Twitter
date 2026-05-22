using System;
using System.Collections.Generic;

namespace Twitter.Domain.Database.SqlServer.Entities;

public partial class Post
{
    public Guid PostId { get; set; }

    public Guid UserId { get; set; }

    public string Content { get; set; } = null!;

    public bool IsPublished { get; set; }

    public int ReportCount { get; set; }

    public bool IsFlagged { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedByAdminId { get; set; }

    public string? DeletedReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<PostMedia> PostMedias { get; set; } = new List<PostMedia>();
}