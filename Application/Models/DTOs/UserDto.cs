namespace Application.Models.DTOs;

public class UserDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsActive { get; set; }
    public bool IsSuspended { get; set; }
    public bool IsShadowBanned { get; set; }
    public DateTime? DeletedAt { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}