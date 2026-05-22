using System.ComponentModel.DataAnnotations;
using Shared.Constants;

namespace Application.Models.Requests.Post;

public class UpdatePostRequest
{
    public Guid? UserId { get; set; }

    [MaxLength(500, ErrorMessage = ValidationConstants.MAX_LENGTH)]
    [MinLength(3, ErrorMessage = ValidationConstants.MIN_LENGTH)]
    public string? Content { get; set; }

    public List<Guid>? MediaIds { get; set; }
}
