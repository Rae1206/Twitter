using System.ComponentModel.DataAnnotations;
using Shared.Constants;

namespace Application.Models.Requests.User;

public class UpdateUserRequest
{
    [MaxLength(150, ErrorMessage = ValidationConstants.MAX_LENGTH)]
    [MinLength(10, ErrorMessage = ValidationConstants.MIN_LENGTH)]
    public string? FullName { get; set; }

    [EmailAddress]
    [MaxLength(255, ErrorMessage = ValidationConstants.MAX_LENGTH)]
    [MinLength(5, ErrorMessage = ValidationConstants.MIN_LENGTH)]
    public string? Email { get; set; }

    [MaxLength(500, ErrorMessage = ValidationConstants.MAX_LENGTH)]
    public string? Biography { get; set; }
}
