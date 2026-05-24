using System.ComponentModel.DataAnnotations;
using Shared.Constants;

namespace Application.Models.Requests.Post;

public class CreatePostRequest
{
    [Required(ErrorMessage = ValidationConstants.REQUIRED)]
    [MaxLength(500, ErrorMessage = ValidationConstants.MAX_LENGTH)]
    [MinLength(3, ErrorMessage = ValidationConstants.MIN_LENGTH)]
    public string Content { get; set; } = null!;

    public bool? IsPublished { get; set; }

    public List<Guid>? MediaIds { get; set; }

    /// <summary>
    /// Duración del post efímero en minutos. Si es null, el post no expira.
    /// Rango permitido: <see cref="PostConstants.MinEphemeralMinutes"/> a <see cref="PostConstants.MaxEphemeralMinutes"/> (1 min a 72 h).
    /// </summary>
    [Range(PostConstants.MinEphemeralMinutes, PostConstants.MaxEphemeralMinutes,
        ErrorMessage = "La duración del post efímero debe estar entre {1} y {2} minutos")]
    public int? DurationMinutes { get; set; }
}
