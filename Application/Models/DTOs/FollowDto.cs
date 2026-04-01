namespace Application.Models.DTOs;

public class FollowDto
{
    public Guid FollowId { get; set; }
    public Guid FollowerId { get; set; }
    public Guid FollowingId { get; set; }
    public string FollowerUsername { get; set; } = string.Empty;
    public string FollowerFullName { get; set; } = string.Empty;
    public string? FollowerAvatar { get; set; }
    public DateTime CreatedAt { get; set; }
}