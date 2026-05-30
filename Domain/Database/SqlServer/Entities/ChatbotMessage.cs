namespace Twitter.Domain.Database.SqlServer.Entities;

public partial class ChatbotMessage
{
    public Guid ChatbotMessageId { get; set; }

    public Guid UserId { get; set; }

    public string Role { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string? Model { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
