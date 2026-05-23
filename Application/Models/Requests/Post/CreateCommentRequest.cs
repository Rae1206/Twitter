using System;
using System.ComponentModel.DataAnnotations;
using Shared.Constants;

namespace Application.Models.Requests.Post;

public class CreateCommentRequest
{
    public Guid UserId { get; set; }

    [Required(ErrorMessage = ValidationConstants.REQUIRED)]
    [MaxLength(ValidationConstants.MAX_POST_LENGTH, ErrorMessage = ValidationConstants.MAX_LENGTH)]
    public string Content { get; set; } = null!;
}
