namespace TalentInsights.Application.Models.DTOs
{
    public class CollaboratorDto
    {
        public Guid CollaboratorId { get; set; }
        public string FullName { get; set; } = null!;
        public string? GitlabProfile { get; set; }
        public string Position { get; set; } = null!;
        public DateTime JoinedAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
