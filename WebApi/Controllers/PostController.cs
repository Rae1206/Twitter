using System;
using System.Threading.Tasks;
using Application.Interfaces.Services;
using Application.Models.Requests.Post;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PostController(
    IPostService postService,
    ILikeService likeService,
    ICommentService commentService,
    IRetweetService retweetService) : ApiControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest model)
    {
        model.UserId = TryGetCurrentUserId() ?? model.UserId;

        var post = await postService.Create(model);
        return CreatedEnvelope(nameof(GetPostById), new { id = post.PostId }, post);
    }

    [HttpGet("list")]
    public IActionResult GetAllPosts([FromQuery] GetAllPostRequest model)
    {
        var rsp = postService.Get(model.Limit ?? 0, model.Offset ?? 0, model.UserId, model.IsPublished);
        return OkEnvelope(rsp);
    }

    [HttpGet("{id:guid}")]
    public IActionResult GetPostById(Guid id)
    {
        var post = postService.Get(id);
        return OkEnvelope(post);
    }

    [HttpPut("{id:guid}/update")]
    public async Task<IActionResult> UpdatePost([FromBody] UpdatePostRequest model, Guid id)
    {
        model.UserId ??= TryGetCurrentUserId();

        var post = await postService.Update(id, model);
        return OkEnvelope(post);
    }

    [HttpPatch("{id:guid}/change-status")]
    public async Task<IActionResult> ChangePostStatus(Guid id, [FromBody] ChangePostStatusRequest model)
    {
        await postService.ChangeStatus(id, model);
        return SuccessEnvelope("Estado de la publicación actualizado correctamente");
    }

    [HttpDelete("{id:guid}/delete")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        await postService.Delete(id);
        return SuccessEnvelope("Publicación eliminada correctamente");
    }

    [Authorize]
    [HttpPost("{id:guid}/like")]
    public async Task<IActionResult> ToggleLike(Guid id)
    {
        var userId = GetRequiredCurrentUserId();
        await likeService.ToggleLike(id, userId);
        return SuccessEnvelope("Reacción de me gusta procesada correctamente");
    }

    [Authorize]
    [HttpPost("{id:guid}/comment")]
    public async Task<IActionResult> CreateComment(Guid id, [FromBody] CreateCommentRequest model)
    {
        var userId = GetRequiredCurrentUserId();
        var post = await commentService.CreateComment(id, userId, model);
        return CreatedEnvelope(nameof(GetPostById), new { id = post.PostId }, post);
    }

    [Authorize]
    [HttpPost("{id:guid}/retweet")]
    public async Task<IActionResult> CreateRetweet(Guid id, [FromBody] CreateRetweetRequest model)
    {
        var userId = GetRequiredCurrentUserId();
        var post = await retweetService.CreateRetweet(id, userId, model);
        return CreatedEnvelope(nameof(GetPostById), new { id = post.PostId }, post);
    }
}
