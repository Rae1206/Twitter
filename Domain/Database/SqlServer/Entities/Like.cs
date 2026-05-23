using System;

namespace Twitter.Domain.Database.SqlServer.Entities;

public partial class Like
{
    public Guid LikeId { get; set; }

    public Guid UserId { get; set; }

    public Guid PostId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual Post Post { get; set; } = null!;
}
