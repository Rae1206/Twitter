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
    /// <summary>
    /// Crea una nueva publicación para el usuario autenticado.
    /// </summary>
    /// <param name="model">Modelo que contiene el contenido de la publicación y configuraciones opcionales.</param>
    /// <returns>La publicación creada con su ID correspondiente.</returns>
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

    /// <summary>
    /// Genera sugerencias de texto para una publicación utilizando Inteligencia Artificial.
    /// </summary>
    /// <param name="model">Modelo de solicitud con parámetros o sugerencias de contexto para la IA.</param>
    /// <returns>El texto sugerido por la IA.</returns>
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

    /// <summary>
    /// Obtiene una lista paginada de publicaciones según los filtros especificados.
    /// </summary>
    /// <param name="model">Modelo de solicitud con parámetros de paginación y filtros.</param>
    /// <returns>Una lista paginada de publicaciones.</returns>
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

    /// <summary>
    /// Obtiene una publicación específica por su identificador único.
    /// </summary>
    /// <param name="id">Identificador único de la publicación.</param>
    /// <returns>Los detalles de la publicación solicitada.</returns>
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

    /// <summary>
    /// Actualiza el contenido de una publicación existente.
    /// </summary>
    /// <param name="model">Modelo con el nuevo contenido para la publicación.</param>
    /// <param name="id">Identificador único de la publicación a actualizar.</param>
    /// <returns>La publicación actualizada.</returns>
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

    /// <summary>
    /// Cambia el estado (de publicación a borrador o viceversa) de una publicación.
    /// </summary>
    /// <param name="id">Identificador único de la publicación.</param>
    /// <param name="model">Modelo que especifica el nuevo estado de publicación.</param>
    /// <returns>Una respuesta indicando el éxito del cambio de estado.</returns>
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

    /// <summary>
    /// Elimina una publicación del usuario autenticado de forma lógica o física.
    /// </summary>
    /// <param name="id">Identificador único de la publicación a eliminar.</param>
    /// <returns>Una respuesta indicando el éxito de la eliminación.</returns>
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

    /// <summary>
    /// Alterna el estado "Me gusta" (like) en una publicación específica.
    /// </summary>
    /// <param name="id">Identificador único de la publicación.</param>
    /// <returns>Una respuesta indicando que la reacción fue procesada correctamente.</returns>
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

    /// <summary>
    /// Crea un comentario respondiendo a una publicación existente.
    /// </summary>
    /// <param name="id">Identificador único de la publicación que se comenta.</param>
    /// <param name="model">Modelo con el contenido del comentario.</param>
    /// <returns>La nueva publicación o comentario creado.</returns>
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

    /// <summary>
    /// Crea un retweet (republicación) de una publicación existente.
    /// </summary>
    /// <param name="id">Identificador único de la publicación a retuitear.</param>
    /// <param name="model">Modelo que contiene comentarios opcionales (retweet con cita).</param>
    /// <returns>El nuevo post que representa el retweet.</returns>
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
