using System;
using System.Threading.Tasks;
using Application.Interfaces.Services;
using Application.Models.Requests.Post;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

/// <summary>
/// Controlador para gestionar publicaciones, likes, comentarios y retweets.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[Tags("Publicaciones")]
public class PostController(
    IPostService postService,
    IPostTextGenerationService postTextGenerationService,
    ILikeService likeService,
    ICommentService commentService,
    IRetweetService retweetService) : ApiControllerBase
{
    [HttpPost("create")]
    [EndpointSummary("Crear una publicación")]
    [EndpointDescription("Crea una nueva publicación para el usuario autenticado.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest model)
    {
        var currentUserId = GetRequiredCurrentUserId();
        var post = await postService.Create(currentUserId, model);
        return CreatedEnvelope(nameof(GetPostById), new { id = post.PostId }, post);
    }

    [HttpPost("generate-text")]
    [EndpointSummary("Generar texto sugerido con IA")]
    [EndpointDescription("Genera un texto sugerido para una publicación usando inteligencia artificial.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerateText([FromBody] GeneratePostTextRequest model)
    {
        var currentUserId = GetRequiredCurrentUserId();
        var generatedPost = await postTextGenerationService.GenerateAsync(currentUserId, model, HttpContext.RequestAborted);
        return OkEnvelope(generatedPost, "Texto sugerido generado correctamente");
    }

    [AllowAnonymous]
    [HttpGet("list")]
    [EndpointSummary("Listar publicaciones")]
    [EndpointDescription("Obtiene una lista paginada de publicaciones. No requiere autenticación.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAllPosts([FromQuery] GetAllPostRequest model)
    {
        var rsp = postService.Get(model.Limit ?? 0, model.Offset ?? 0, model.UserId, model.IsPublished);
        return OkEnvelope(rsp);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    [EndpointSummary("Obtener publicación por ID")]
    [EndpointDescription("Obtiene una publicación específica por su identificador.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPostById(Guid id)
    {
        var post = await postService.Get(id);
        return OkEnvelope(post);
    }

    [HttpPut("{id:guid}/update")]
    [EndpointSummary("Actualizar publicación")]
    [EndpointDescription("Actualiza el contenido de una publicación existente del usuario autenticado.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePost([FromBody] UpdatePostRequest model, Guid id)
    {
        var currentUserId = GetRequiredCurrentUserId();
        var post = await postService.Update(id, currentUserId, model);
        return OkEnvelope(post);
    }

    [HttpPatch("{id:guid}/change-status")]
    [EndpointSummary("Cambiar estado de publicación")]
    [EndpointDescription("Cambia el estado (publicado/borrador) de una publicación del usuario autenticado.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePostStatus(Guid id, [FromBody] ChangePostStatusRequest model)
    {
        var currentUserId = GetRequiredCurrentUserId();
        await postService.ChangeStatus(id, currentUserId, model);
        return SuccessEnvelope("Estado de la publicación actualizado correctamente");
    }

    [HttpDelete("{id:guid}/delete")]
    [EndpointSummary("Eliminar publicación")]
    [EndpointDescription("Elimina una publicación del usuario autenticado.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        var currentUserId = GetRequiredCurrentUserId();
        await postService.Delete(id, currentUserId);
        return SuccessEnvelope("Publicación eliminada correctamente");
    }

    [HttpPost("{id:guid}/like")]
    [EndpointSummary("Dar o quitar like")]
    [EndpointDescription("Alterna el like en una publicación. Si ya tiene like lo quita, si no lo agrega.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleLike(Guid id)
    {
        var userId = GetRequiredCurrentUserId();
        await likeService.ToggleLike(id, userId);
        return SuccessEnvelope("Reacción de me gusta procesada correctamente");
    }

    [HttpPost("{id:guid}/comment")]
    [EndpointSummary("Comentar publicación")]
    [EndpointDescription("Crea un comentario en una publicación existente.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateComment(Guid id, [FromBody] CreateCommentRequest model)
    {
        var userId = GetRequiredCurrentUserId();
        var post = await commentService.CreateComment(id, userId, model);
        return CreatedEnvelope(nameof(GetPostById), new { id = post.PostId }, post);
    }

    [HttpPost("{id:guid}/retweet")]
    [EndpointSummary("Hacer retweet")]
    [EndpointDescription("Crea un retweet de una publicación existente.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateRetweet(Guid id, [FromBody] CreateRetweetRequest model)
    {
        var userId = GetRequiredCurrentUserId();
        var post = await retweetService.CreateRetweet(id, userId, model);
        return CreatedEnvelope(nameof(GetPostById), new { id = post.PostId }, post);
    }
}
