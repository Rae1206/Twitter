using Application.Models.DTOs;

namespace Application.Interfaces.Repositories;

public interface IPostRepository
{
    PostDto Create(PostDto post);
    PostDto? GetById(Guid postId);
    List<PostDto> GetAll(int limit, int offset, Guid? userId, bool? isPublished);
    PostDto? Update(Guid postId, PostDto post);
    bool Delete(Guid postId);
    bool ChangeStatus(Guid postId, bool isPublished);
}