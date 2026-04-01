namespace Application.Models.DTOs;

public class CommentDto
{
    public Guid CommentId { get; set; }
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public string? Username { get; set; }
    public string Content { get; set; } = null!;
    public int LikesCount { get; set; }
    public DateTime CreatedAt { get; set; }
}