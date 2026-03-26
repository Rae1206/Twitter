namespace TalentInsights.Application.Models.Requests.Collaborator
{
    public class GetAllColaboratorRequest
    {
        public int? Limit { get; set; }
        public int? Offset { get; set; }
        public string? GitlabProfile { get; set; }
        public string? FullName { get; set; }
    }
}
