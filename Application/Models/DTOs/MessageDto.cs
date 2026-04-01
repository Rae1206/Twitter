namespace Application.Models.DTOs;

public class MessageDto
{
    public Guid MessageId { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public string SenderUsername { get; set; } = string.Empty;
    public string ReceiverUsername { get; set; } = string.Empty;
    public string? SenderAvatar { get; set; }
    public string? ReceiverAvatar { get; set; }
    public string Content { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}