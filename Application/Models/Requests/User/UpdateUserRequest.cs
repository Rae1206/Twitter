using System.ComponentModel.DataAnnotations;
using Shared.Constants;

namespace Application.Models.Requests.User;

public class UpdateUserRequest
{
    [MaxLength(30, ErrorMessage = ValidationConstants.MAX_LENGTH)]
    [MinLength(2, ErrorMessage = ValidationConstants.MIN_LENGTH)]
    public string? Nickname { get; set; }

    [EmailAddress]
    [MaxLength(255, ErrorMessage = ValidationConstants.MAX_LENGTH)]
    [MinLength(5, ErrorMessage = ValidationConstants.MIN_LENGTH)]
    public string? Email { get; set; }

    [MaxLength(500, ErrorMessage = ValidationConstants.MAX_LENGTH)]
    public string? Biography { get; set; }
}
