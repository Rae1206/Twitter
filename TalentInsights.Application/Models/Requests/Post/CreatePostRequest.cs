using System.ComponentModel.DataAnnotations;
using TalentInsights.Shared.Constants;

namespace TalentInsights.Application.Models.Requests.Post;

public class CreatePostRequest
{
    [Required(ErrorMessage = ValidationConstants.REQUIRED)]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = ValidationConstants.REQUIRED)]
    [MaxLength(500, ErrorMessage = ValidationConstants.MAX_LENGTH)]
    [MinLength(3, ErrorMessage = ValidationConstants.MIN_LENGTH)]
    public string Content { get; set; } = null!;
}
