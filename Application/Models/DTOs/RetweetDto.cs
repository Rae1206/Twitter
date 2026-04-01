namespace Application.Models.DTOs;

public class RetweetDto
{
    public Guid RetweetId { get; set; }
    public Guid OriginalPostId { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public string? Username { get; set; }
    public string? Comment { get; set; }  // Quote retweet
    public int LikesCount { get; set; }
    public DateTime CreatedAt { get; set; }
}