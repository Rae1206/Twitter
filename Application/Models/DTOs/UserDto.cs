namespace Application.Models.DTOs;

public class UserDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Biography { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? ProfilePhotoFileName { get; set; }
    public bool IsActive { get; set; }
    public bool IsSuspended { get; set; }
    public bool IsShadowBanned { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
