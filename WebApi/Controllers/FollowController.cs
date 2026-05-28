using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Attributes;

namespace WebApi.Controllers;

[Route("api/follow")]
[ApiController]
[Authorize]
[RequireNotSuspended]
public class FollowController(IFollowService followService) : ApiControllerBase
{
    /// <summary>
    /// Seguir a un usuario
    /// </summary>
    [HttpPost("{userId:guid}/follow")]
    public async Task<IActionResult> Follow(Guid userId)
    {
        var currentUserId = GetRequiredCurrentUserId();
        await followService.FollowUser(currentUserId, userId);
        return SuccessEnvelope("Ahora sigues a este usuario");
    }

    /// <summary>
    /// Dejar de seguir a un usuario
    /// </summary>
    [HttpDelete("{userId:guid}/unfollow")]
    public async Task<IActionResult> Unfollow(Guid userId)
    {
        var currentUserId = GetRequiredCurrentUserId();
        await followService.UnfollowUser(currentUserId, userId);
        return SuccessEnvelope("Dejaste de seguir a este usuario");
    }

    /// <summary>
    /// Verificar si sigues a un usuario
    /// </summary>
    [HttpGet("{userId:guid}/is-following")]
    public async Task<IActionResult> IsFollowing(Guid userId)
    {
        var currentUserId = GetRequiredCurrentUserId();
        var isFollowing = await followService.IsFollowing(currentUserId, userId);
        return OkEnvelope(new { isFollowing });
    }

    /// <summary>
    /// Obtener seguidores de un usuario
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{userId:guid}/followers")]
    public async Task<IActionResult> GetFollowers(Guid userId, [FromQuery] int limit = 0, [FromQuery] int offset = 0)
    {
        var followers = await followService.GetFollowers(userId, limit, offset);
        return OkEnvelope(followers);
    }

    /// <summary>
    /// Obtener usuarios que sigue
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{userId:guid}/following")]
    public async Task<IActionResult> GetFollowing(Guid userId, [FromQuery] int limit = 0, [FromQuery] int offset = 0)
    {
        var following = await followService.GetFollowing(userId, limit, offset);
        return OkEnvelope(following);
    }

    /// <summary>
    /// Obtener cantidad de seguidores
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{userId:guid}/followers/count")]
    public async Task<IActionResult> GetFollowersCount(Guid userId)
    {
        var count = await followService.GetFollowersCount(userId);
        return OkEnvelope(count);
    }

    /// <summary>
    /// Obtener cantidad de usuarios que sigue
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{userId:guid}/following/count")]
    public async Task<IActionResult> GetFollowingCount(Guid userId)
    {
        var count = await followService.GetFollowingCount(userId);
        return OkEnvelope(count);
    }
}
