using Application.Models.DTOs;
using Application.Models.Requests.Post;

namespace Application.Interfaces.Services;

public interface IPostTextGenerationService
{
    Task<GeneratedPostTextDto> GenerateAsync(
        Guid currentUserId,
        GeneratePostTextRequest model,
        CancellationToken cancellationToken = default);
}
