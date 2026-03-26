namespace TalentInsights.Application.Models.Requests.User;

public class GetAllUserRequest
{
    public int? Limit { get; set; }
    public int? Offset { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
}
