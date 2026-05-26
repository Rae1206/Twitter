using System;

namespace Twitter.Domain.Database.SqlServer.Entities;

public partial class Follow
{
    public Guid FollowId { get; set; }

    public Guid FollowerId { get; set; }

    public Guid FollowingId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User Follower { get; set; } = null!;

    public virtual User Following { get; set; } = null!;
}