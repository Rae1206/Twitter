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

    public Guid? RepliedToPostId { get; set; }

    public virtual Post? RepliedToPost { get; set; }

    public virtual ICollection<Post> Replies { get; set; } = new List<Post>();

    public Guid? RetweetOfPostId { get; set; }

    public virtual Post? RetweetOfPost { get; set; }

    public virtual ICollection<Post> Retweets { get; set; } = new List<Post>();

    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
}