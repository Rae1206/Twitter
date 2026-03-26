using System.ComponentModel.DataAnnotations;
using TalentInsights.Shared.Constants;

namespace TalentInsights.Application.Models.Requests.Collaborator
{
    public class UpdateCollaboratorRequest
    {
        [MaxLength(150, ErrorMessage = ValidationConstants.MAX_LENGTH)]
        [MinLength(10, ErrorMessage = ValidationConstants.MIN_LENGTH)]
        public string? FullName { get; set; }

        [MaxLength(255, ErrorMessage = ValidationConstants.MAX_LENGTH)]
        [MinLength(10, ErrorMessage = ValidationConstants.MIN_LENGTH)]
        public string? GitlabProfile { get; set; }

        [MaxLength(100, ErrorMessage = ValidationConstants.MAX_LENGTH)]
        [MinLength(5, ErrorMessage = ValidationConstants.MIN_LENGTH)]
        public string? Position { get; set; }
    }
}
