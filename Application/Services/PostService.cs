using Application.Helpers;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models.DTOs;
using Application.Models.Requests.Post;
using Application.Models.Responses;
using Shared.Helpers;

namespace Application.Services;

public class PostService(IPostRepository postRepository, IUserRepository userRepository) : IPostService
{
    public GenericResponse<PostDto> Create(CreatePostRequest model)
    {
        var user = userRepository.GetById(model.UserId);

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

        var created = postRepository.Create(post);
        return ResponseHelper.Create(MapWithUserName(created));
    }

    public GenericResponse<PostDto> Update(Guid postId, UpdatePostRequest model)
    {
        var post = postRepository.GetById(postId);

        if (post is null)
        {
            return ResponseHelper.Create(new PostDto(), "Post no encontrado");
        }

        if (model.UserId.HasValue)
        {
            var user = userRepository.GetById(model.UserId.Value);
            if (user is null)
            {
                return ResponseHelper.Create(new PostDto(), "Usuario no encontrado");
            }
        }

        var updatedPost = new PostDto
        {
            PostId = post.PostId,
            UserId = model.UserId ?? post.UserId,
            Content = model.Content ?? post.Content,
            IsPublished = post.IsPublished,
            CreatedAt = post.CreatedAt
        };

        var result = postRepository.Update(postId, updatedPost);
        return result is null 
            ? ResponseHelper.Create(new PostDto(), "Error al actualizar")
            : ResponseHelper.Create(MapWithUserName(result));
    }

    public GenericResponse<List<PostDto>> Get(int limit, int offset, Guid? userId, bool? isPublished)
    {
        var posts = postRepository.GetAll(limit, offset, userId, isPublished);
        var result = posts.Select(MapWithUserName).ToList();
        return ResponseHelper.Create(result);
    }

    public GenericResponse<PostDto?> Get(Guid postId)
    {
        var post = postRepository.GetById(postId);
        return ResponseHelper.Create(post is null ? null : MapWithUserName(post));
    }

    public GenericResponse<bool> ChangeStatus(Guid postId, ChangePostStatusRequest model)
    {
        var post = postRepository.GetById(postId);

        if (post is null)
        {
            return ResponseHelper.Create(false, "Post no encontrado");
        }

        var result = postRepository.ChangeStatus(postId, model.IsPublished);
        return result 
            ? ResponseHelper.Create(true, "Estado del post actualizado correctamente")
            : ResponseHelper.Create(false, "Error al cambiar estado");
    }

    public GenericResponse<bool> Delete(Guid postId)
    {
        var post = postRepository.GetById(postId);

        if (post is null)
        {
            return ResponseHelper.Create(false);
        }

        var result = postRepository.Delete(postId);
        return result ? ResponseHelper.Create(true) : ResponseHelper.Create(false);
    }

    private PostDto MapWithUserName(PostDto post)
    {
        var user = userRepository.GetById(post.UserId);

        return new PostDto
        {
            PostId = post.PostId,
            UserId = post.UserId,
            UserFullName = user?.FullName ?? "Usuario no encontrado",
            Content = post.Content,
            IsPublished = post.IsPublished,
            CreatedAt = post.CreatedAt
        };
    }
}