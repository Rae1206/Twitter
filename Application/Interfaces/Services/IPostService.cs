using Application.Models.DTOs;
using Application.Models.Requests.Post;
using Application.Models.Responses;

namespace Application.Interfaces.Services;

public interface IPostService
{
    PostDto Create(CreatePostRequest model);
    PostDto Update(Guid postId, UpdatePostRequest model);
    GenericResponse<List<PostDto>> Get(int limit, int offset, Guid? userId, bool? isPublished);
    PostDto Get(Guid postId);
    void ChangeStatus(Guid postId, ChangePostStatusRequest model);
    void Delete(Guid postId);
}
