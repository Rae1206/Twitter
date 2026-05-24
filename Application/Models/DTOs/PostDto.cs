namespace Application.Models.DTOs;

public class PostDto
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public string? Username { get; set; }
    public string Content { get; set; } = null!;
    public Guid? RepliedToPostId { get; set; }
    public Guid? RetweetOfPostId { get; set; }
    public bool IsPublished { get; set; }
    public int ReportCount { get; set; }
    public bool IsFlagged { get; set; }
    public string? DeletedReason { get; set; }
    public int LikesCount { get; set; }
    public int RetweetsCount { get; set; }
    public int RepliesCount { get; set; }
    public List<string>? MediaUrls { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha (UTC) de expiración del post efímero. Si es null, el post no expira.
    /// El frontend puede usar esto para mostrar un countdown.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}