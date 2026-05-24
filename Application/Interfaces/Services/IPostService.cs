using Application.Models.DTOs;
using Application.Models.Requests.Post;
using Application.Models.Responses;

namespace Application.Interfaces.Services;

public interface IPostService
{
    Task<PostDto> Create(Guid currentUserId, CreatePostRequest model);
    Task<PostDto> Update(Guid postId, Guid currentUserId, UpdatePostRequest model);
    GenericResponse<List<PostDto>> Get(int limit, int offset, Guid? userId, bool? isPublished);
    Task<PostDto> Get(Guid postId);
    Task ChangeStatus(Guid postId, Guid currentUserId, ChangePostStatusRequest model);
    Task Delete(Guid postId, Guid currentUserId);
}
