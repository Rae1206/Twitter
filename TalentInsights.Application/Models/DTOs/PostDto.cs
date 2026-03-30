namespace TalentInsights.Application.Models.DTOs;

public class PostDto
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string Content { get; set; } = null!;
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
}
