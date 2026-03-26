using System.ComponentModel.DataAnnotations;
using TalentInsights.Shared.Constants;

namespace TalentInsights.Application.Models.Requests.User;

public class ChangePasswordUserRequest
{
    [Required(ErrorMessage = ValidationConstants.REQUIRED)]
    public string CurrentPassword { get; set; } = null!;

    [Required(ErrorMessage = ValidationConstants.REQUIRED)]
    public string NewPassword { get; set; } = null!;
}
