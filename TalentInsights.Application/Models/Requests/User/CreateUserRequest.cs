using System.ComponentModel.DataAnnotations;
using TalentInsights.Shared.Constants;

namespace TalentInsights.Application.Models.Requests.User;

public class CreateUserRequest
{
    [Required(ErrorMessage = ValidationConstants.REQUIRED)]
    [MaxLength(150, ErrorMessage = ValidationConstants.MAX_LENGTH)]
    [MinLength(10, ErrorMessage = ValidationConstants.MIN_LENGTH)]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = ValidationConstants.REQUIRED)]
    [EmailAddress]
    [MaxLength(255, ErrorMessage = ValidationConstants.MAX_LENGTH)]
    [MinLength(5, ErrorMessage = ValidationConstants.MIN_LENGTH)]
    public string Email { get; set; } = null!;
}
