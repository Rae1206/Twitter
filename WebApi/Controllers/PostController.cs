using System;
using System.Threading.Tasks;
using Application.Interfaces.Services;
using Application.Models.Requests.Post;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PostController(
    IPostService postService,
    ILikeService likeService,
    ICommentService commentService,
    IRetweetService retweetService) : ApiControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest model)
    {
        var currentUserId = GetRequiredCurrentUserId();
        var post = await postService.Create(currentUserId, model);
        return CreatedEnvelope(nameof(GetPostById), new { id = post.PostId }, post);
    }

    [AllowAnonymous]
    [HttpGet("list")]
    public IActionResult GetAllPosts([FromQuery] GetAllPostRequest model)
    {
        var rsp = postService.Get(model.Limit ?? 0, model.Offset ?? 0, model.UserId, model.IsPublished);
        return OkEnvelope(rsp);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPostById(Guid id)
    {
        var post = await postService.Get(id);
        return OkEnvelope(post);
    }

    [HttpPut("{id:guid}/update")]
    public async Task<IActionResult> UpdatePost([FromBody] UpdatePostRequest model, Guid id)
    {
        var currentUserId = GetRequiredCurrentUserId();
        var post = await postService.Update(id, currentUserId, model);
        return OkEnvelope(post);
    }

    [HttpPatch("{id:guid}/change-status")]
    public async Task<IActionResult> ChangePostStatus(Guid id, [FromBody] ChangePostStatusRequest model)
    {
        var currentUserId = GetRequiredCurrentUserId();
        await postService.ChangeStatus(id, currentUserId, model);
        return SuccessEnvelope("Estado de la publicación actualizado correctamente");
    }

    [HttpDelete("{id:guid}/delete")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        var currentUserId = GetRequiredCurrentUserId();
        await postService.Delete(id, currentUserId);
        return SuccessEnvelope("Publicación eliminada correctamente");
    }

    [HttpPost("{id:guid}/like")]
    public async Task<IActionResult> ToggleLike(Guid id)
    {
        var userId = GetRequiredCurrentUserId();
        await likeService.ToggleLike(id, userId);
        return SuccessEnvelope("Reacción de me gusta procesada correctamente");
    }

    [HttpPost("{id:guid}/comment")]
    public async Task<IActionResult> CreateComment(Guid id, [FromBody] CreateCommentRequest model)
    {
        var userId = GetRequiredCurrentUserId();
        var post = await commentService.CreateComment(id, userId, model);
        return CreatedEnvelope(nameof(GetPostById), new { id = post.PostId }, post);
    }

    [HttpPost("{id:guid}/retweet")]
    public async Task<IActionResult> CreateRetweet(Guid id, [FromBody] CreateRetweetRequest model)
    {
        var userId = GetRequiredCurrentUserId();
        var post = await retweetService.CreateRetweet(id, userId, model);
        return CreatedEnvelope(nameof(GetPostById), new { id = post.PostId }, post);
    }
}
