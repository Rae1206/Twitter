using System.ComponentModel.DataAnnotations;
using Shared.Constants;

namespace Application.Models.Requests.User;

public class CreateUserRequest
{
    [Required(ErrorMessage = ValidationConstants.REQUIRED)]
    [MaxLength(30, ErrorMessage = ValidationConstants.MAX_LENGTH)]
    [MinLength(2, ErrorMessage = ValidationConstants.MIN_LENGTH)]
    public string Nickname { get; set; } = null!;

    [Required(ErrorMessage = ValidationConstants.REQUIRED)]
    [EmailAddress]
    [MaxLength(255, ErrorMessage = ValidationConstants.MAX_LENGTH)]
    [MinLength(5, ErrorMessage = ValidationConstants.MIN_LENGTH)]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = ValidationConstants.REQUIRED)]
    [MinLength(8, ErrorMessage = ValidationConstants.MIN_LENGTH)]
    [MaxLength(100, ErrorMessage = ValidationConstants.MAX_LENGTH)]
    public string Password { get; set; } = null!;
}
