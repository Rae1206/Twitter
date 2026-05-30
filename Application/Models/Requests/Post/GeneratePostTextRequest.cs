using System.ComponentModel.DataAnnotations;
using Shared.Constants;

namespace Application.Models.Requests.Post;

public class GeneratePostTextRequest
{
    [Required(ErrorMessage = ValidationConstants.REQUIRED)]
    [MinLength(1, ErrorMessage = ValidationConstants.MIN_LENGTH)]
    [MaxLength(1000, ErrorMessage = ValidationConstants.MAX_LENGTH)]
    public string Idea { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = ValidationConstants.MAX_LENGTH)]
    public string? Tone { get; set; }

    [Range(1, ValidationConstants.MAX_POST_LENGTH, ErrorMessage = ValidationConstants.INVALID_RANGE)]
    public int? MaxLength { get; set; }
}
