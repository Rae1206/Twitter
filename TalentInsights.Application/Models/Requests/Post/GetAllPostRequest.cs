namespace TalentInsights.Application.Models.Requests.Post;

public class GetAllPostRequest
{
    public int? Limit { get; set; }
    public int? Offset { get; set; }
    public Guid? UserId { get; set; }
    public bool? IsPublished { get; set; }
}
