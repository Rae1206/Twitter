namespace Application.Models.DTOs;

public class LikeDto
{
    public Guid LikeId { get; set; }
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public DateTime CreatedAt { get; set; }
}