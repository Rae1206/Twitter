using System.ComponentModel.DataAnnotations;
using TalentInsights.Shared.Constants;

namespace TalentInsights.Application.Models.Requests.Collaborator
{
    public class ChangePasswordCollaboratorRequest
    {
        [Required(ErrorMessage = ValidationConstants.REQUIRED)]
        public string CurrentPassword { get; set; } = null!;
        [Required(ErrorMessage = ValidationConstants.REQUIRED)]
        public string NewPassword { get; set; } = null!;
    }
}
