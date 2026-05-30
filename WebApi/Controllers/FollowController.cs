using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Attributes;

namespace WebApi.Controllers;

/// <summary>
/// Controlador para gestionar las relaciones de seguimiento (seguidores y seguidos).
/// </summary>
[Route("api/follow")]
[ApiController]
[Authorize]
[RequireNotSuspended]
[Tags("Seguimientos")]
public class FollowController(IFollowService followService) : ApiControllerBase
{
    [HttpPost("{userId:guid}/follow")]
    [EndpointSummary("Seguir a un usuario")]
    [EndpointDescription("Permite al usuario autenticado seguir a otro usuario específico.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Follow(Guid userId)
    {
        var currentUserId = GetRequiredCurrentUserId();
        await followService.FollowUser(currentUserId, userId);
        return SuccessEnvelope("Ahora sigues a este usuario");
    }

    [HttpDelete("{userId:guid}/unfollow")]
    [EndpointSummary("Dejar de seguir a un usuario")]
    [EndpointDescription("Permite al usuario autenticado dejar de seguir a otro usuario específico.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unfollow(Guid userId)
    {
        var currentUserId = GetRequiredCurrentUserId();
        await followService.UnfollowUser(currentUserId, userId);
        return SuccessEnvelope("Dejaste de seguir a este usuario");
    }

    [HttpGet("{userId:guid}/is-following")]
    [EndpointSummary("Verificar si se sigue a un usuario")]
    [EndpointDescription("Verifica si el usuario autenticado sigue al usuario especificado.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> IsFollowing(Guid userId)
    {
        var currentUserId = GetRequiredCurrentUserId();
        var isFollowing = await followService.IsFollowing(currentUserId, userId);
        return OkEnvelope(new { isFollowing });
    }

    [AllowAnonymous]
    [HttpGet("{userId:guid}/followers")]
    [EndpointSummary("Obtener seguidores")]
    [EndpointDescription("Obtiene la lista paginada de seguidores de un usuario. No requiere autenticación.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFollowers(Guid userId, [FromQuery] int limit = 0, [FromQuery] int offset = 0)
    {
        var followers = await followService.GetFollowers(userId, limit, offset);
        return OkEnvelope(followers);
    }

    [AllowAnonymous]
    [HttpGet("{userId:guid}/following")]
    [EndpointSummary("Obtener seguidos")]
    [EndpointDescription("Obtiene la lista paginada de usuarios a los que sigue un usuario. No requiere autenticación.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFollowing(Guid userId, [FromQuery] int limit = 0, [FromQuery] int offset = 0)
    {
        var following = await followService.GetFollowing(userId, limit, offset);
        return OkEnvelope(following);
    }

    [AllowAnonymous]
    [HttpGet("{userId:guid}/followers/count")]
    [EndpointSummary("Contar seguidores")]
    [EndpointDescription("Obtiene el número total de seguidores de un usuario.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFollowersCount(Guid userId)
    {
        var count = await followService.GetFollowersCount(userId);
        return OkEnvelope(count);
    }

    [AllowAnonymous]
    [HttpGet("{userId:guid}/following/count")]
    [EndpointSummary("Contar seguidos")]
    [EndpointDescription("Obtiene el número total de usuarios a los que sigue un usuario.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFollowingCount(Guid userId)
    {
        var count = await followService.GetFollowingCount(userId);
        return OkEnvelope(count);
    }
}
