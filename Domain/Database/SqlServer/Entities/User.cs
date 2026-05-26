using System;
using System.Collections.Generic;

namespace Twitter.Domain.Database.SqlServer.Entities;

public partial class User
{
    public Guid UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Biography { get; set; }

    public string? ProfilePhotoFileName { get; set; }

    public string? ProfilePhotoStoragePath { get; set; }

    public string? ProfilePhotoUrl { get; set; }

    public bool IsActive { get; set; }

    public bool IsSuspended { get; set; }

    public DateTime? SuspendedUntil { get; set; }

    public bool IsShadowBanned { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedByAdminId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int FollowersCount { get; set; }

    public int FollowingCount { get; set; }

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

    public virtual ICollection<Follow> Followers { get; set; } = new List<Follow>();

    public virtual ICollection<Follow> Following { get; set; } = new List<Follow>();

    public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();

    public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
}
