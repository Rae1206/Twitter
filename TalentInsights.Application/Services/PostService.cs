using TalentInsights.Application.Helpers;
using TalentInsights.Application.Interfaces.Services;
using TalentInsights.Application.Models.DTOs;
using TalentInsights.Application.Models.Requests.Post;
using TalentInsights.Application.Models.Responses;
using TalentInsights.Shared;
using TalentInsights.Shared.Helpers;

namespace TalentInsights.Application.Services;

public class PostService(Cache<PostDto> postCache, Cache<UserDto> userCache) : IPostService
{
    public GenericResponse<PostDto> Create(CreatePostRequest model)
    {
        var user = userCache.Get(model.UserId.ToString());

        if (user is null)
        {
            return ResponseHelper.Create(new PostDto(), "Usuario no encontrado");
        }

        var post = new PostDto
        {
            PostId = Guid.NewGuid(),
            UserId = model.UserId,
            Content = model.Content,
            IsPublished = true,
            CreatedAt = DateTimeHelper.UtcNow()
        };

        postCache.Add(post.PostId.ToString(), post);
        return ResponseHelper.Create(post);
    }

    public GenericResponse<PostDto> Update(Guid postId, UpdatePostRequest model)
    {
        var post = postCache.Get(postId.ToString());

        if (post is null)
        {
            return ResponseHelper.Create(new PostDto(), "Post no encontrado");
        }

        if (model.UserId.HasValue)
        {
            var user = userCache.Get(model.UserId.Value.ToString());
            if (user is null)
            {
                return ResponseHelper.Create(new PostDto(), "Usuario no encontrado");
            }

            post.UserId = model.UserId.Value;
        }

        if (!string.IsNullOrWhiteSpace(model.Content))
        {
            post.Content = model.Content;
        }

        return ResponseHelper.Create(post);
    }

    public GenericResponse<List<PostDto>> Get(int limit, int offset, Guid? userId, bool? isPublished)
    {
        var posts = postCache.Get().AsEnumerable();

        if (userId.HasValue)
        {
            posts = posts.Where(x => x.UserId == userId.Value);
        }

        if (isPublished.HasValue)
        {
            posts = posts.Where(x => x.IsPublished == isPublished.Value);
        }

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        var result = posts.Skip(normalizedOffset).Take(normalizedLimit).ToList();
        return ResponseHelper.Create(result);
    }

    public GenericResponse<PostDto?> Get(Guid postId)
    {
        var post = postCache.Get(postId.ToString());
        return ResponseHelper.Create(post);
    }

    public GenericResponse<bool> ChangeStatus(Guid postId, ChangePostStatusRequest model)
    {
        var post = postCache.Get(postId.ToString());

        if (post is null)
        {
            return ResponseHelper.Create(false, "Post no encontrado");
        }

        post.IsPublished = model.IsPublished;
        return ResponseHelper.Create(true, "Estado del post actualizado correctamente");
    }

    public GenericResponse<bool> Delete(Guid postId)
    {
        var post = postCache.Get(postId.ToString());

        if (post is null)
        {
            return ResponseHelper.Create(false);
        }

        postCache.Delete(postId.ToString());
        return ResponseHelper.Create(true);
    }
}
