using System;

namespace Twitter.Domain.Database.SqlServer.Entities;

public partial class Message
{
    public Guid MessageId { get; set; }

    public Guid SenderId { get; set; }

    public Guid ReceiverId { get; set; }

    public string Content { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public bool DeletedBySender { get; set; }

    public bool DeletedByReceiver { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User Sender { get; set; } = null!;

    public virtual User Receiver { get; set; } = null!;
}