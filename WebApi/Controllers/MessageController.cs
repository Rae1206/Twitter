using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebApi.Attributes;
using WebApi.Hubs;

namespace WebApi.Controllers;

/// <summary>
/// Controlador para gestionar los mensajes directos (DM) entre usuarios.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
[RequireNotSuspended]
[Tags("Mensajes Directos")]
public class MessageController(
    IMessageService messageService,
    IHubContext<MessageHub> hubContext) : ApiControllerBase
{
    /// <summary>
    /// Envía un mensaje privado a otro usuario y le notifica en tiempo real vía SignalR.
    /// </summary>
    /// <param name="request">Los datos de transferencia con el destinatario y el contenido del mensaje.</param>
    /// <returns>El mensaje directo creado.</returns>
    [HttpPost("send")]
    [EndpointSummary("Enviar un mensaje directo")]
    [EndpointDescription("Envía un mensaje privado a otro usuario y le notifica en tiempo real vía SignalR.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto request)
    {
        var currentUserId = GetRequiredCurrentUserId();
        var message = await messageService.SendMessage(currentUserId, request.ReceiverId, request.Content);
        
        // Enviar notificación en tiempo real al receptor a través de SignalR
        await hubContext.Clients
            .Group($"user-{request.ReceiverId}")
            .SendAsync("ReceiveMessage", message);
        
        return CreatedEnvelope(nameof(GetConversation), new { otherUserId = request.ReceiverId }, message, "Mensaje enviado correctamente");
    }

    /// <summary>
    /// Obtiene los mensajes intercambiados de forma paginada con otro usuario específico.
    /// </summary>
    /// <param name="otherUserId">Identificador único del otro usuario en la conversación.</param>
    /// <param name="limit">Cantidad máxima de mensajes a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>La lista paginada de mensajes en la conversación.</returns>
    [HttpGet("conversation/{otherUserId:guid}")]
    [EndpointSummary("Obtener conversación con un usuario")]
    [EndpointDescription("Obtiene los mensajes intercambiados de forma paginada con otro usuario específico.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetConversation(Guid otherUserId, [FromQuery] int limit = 50, [FromQuery] int offset = 0)
    {
        var currentUserId = GetRequiredCurrentUserId();
        var messages = await messageService.GetConversation(currentUserId, otherUserId, limit, offset);
        return OkEnvelope(messages);
    }

    /// <summary>
    /// Obtiene la lista de conversaciones activas del usuario autenticado con sus últimos mensajes.
    /// </summary>
    /// <param name="limit">Cantidad máxima de conversaciones a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <returns>La lista de conversaciones activas.</returns>
    [HttpGet("conversations")]
    [EndpointSummary("Listar conversaciones")]
    [EndpointDescription("Obtiene una lista de conversaciones activas con sus últimos mensajes recibidos o enviados.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetConversationsList([FromQuery] int limit = 20, [FromQuery] int offset = 0)
    {
        var currentUserId = GetRequiredCurrentUserId();
        var conversations = await messageService.GetConversationsList(currentUserId, limit, offset);
        return OkEnvelope(conversations);
    }

    /// <summary>
    /// Obtiene el número total de mensajes no leídos en una conversación específica con otro usuario.
    /// </summary>
    /// <param name="otherUserId">Identificador único del otro usuario.</param>
    /// <returns>La cantidad de mensajes no leídos.</returns>
    [HttpGet("conversation/{otherUserId:guid}/unread/count")]
    [EndpointSummary("Obtener no leídos de una conversación")]
    [EndpointDescription("Obtiene el número de mensajes no leídos de una conversación específica.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUnreadCountInConversation(Guid otherUserId)
    {
        var currentUserId = GetRequiredCurrentUserId();
        var count = await messageService.GetUnreadCountInConversation(currentUserId, otherUserId);
        return OkEnvelope(new { count });
    }

    /// <summary>
    /// Obtiene todos los mensajes no leídos recibidos por el usuario autenticado.
    /// </summary>
    /// <returns>Una lista con los mensajes no leídos.</returns>
    [HttpGet("unread")]
    [EndpointSummary("Obtener mensajes no leídos")]
    [EndpointDescription("Obtiene todos los mensajes no leídos del usuario autenticado.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUnreadMessages()
    {
        var currentUserId = GetRequiredCurrentUserId();
        var messages = await messageService.GetUnreadMessages(currentUserId);
        return OkEnvelope(messages);
    }

    /// <summary>
    /// Obtiene el número total de mensajes no leídos acumulados de todas las conversaciones del usuario.
    /// </summary>
    /// <returns>La cantidad total acumulada de mensajes no leídos.</returns>
    [HttpGet("unread/count")]
    [EndpointSummary("Contar total de mensajes no leídos")]
    [EndpointDescription("Obtiene el número total de mensajes no leídos acumulados de todas las conversaciones.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUnreadCount()
    {
        var currentUserId = GetRequiredCurrentUserId();
        var count = await messageService.GetUnreadCount(currentUserId);
        return OkEnvelope(new { count });
    }

    /// <summary>
    /// Marca un mensaje directo específico como leído.
    /// </summary>
    /// <param name="messageId">Identificador único del mensaje.</param>
    /// <returns>Una respuesta indicando el éxito del marcado.</returns>
    [HttpPatch("{messageId:guid}/read")]
    [EndpointSummary("Marcar mensaje como leído")]
    [EndpointDescription("Marca un mensaje directo recibido como leído por su identificador.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid messageId)
    {
        var currentUserId = GetRequiredCurrentUserId();
        await messageService.MarkAsRead(messageId, currentUserId);
        return SuccessEnvelope("Mensaje marcado como leído");
    }

    /// <summary>
    /// Marca todos los mensajes recibidos de una conversación con otro usuario como leídos.
    /// </summary>
    /// <param name="otherUserId">Identificador único del otro usuario de la conversación.</param>
    /// <returns>Una respuesta indicando el éxito de la operación.</returns>
    [HttpPatch("conversation/{otherUserId:guid}/read")]
    [EndpointSummary("Marcar conversación como leída")]
    [EndpointDescription("Marca todos los mensajes pendientes de una conversación con otro usuario como leídos.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkConversationAsRead(Guid otherUserId)
    {
        var currentUserId = GetRequiredCurrentUserId();
        await messageService.MarkConversationAsRead(currentUserId, otherUserId);
        return SuccessEnvelope("Conversación marcada como leída");
    }

    /// <summary>
    /// Realiza la eliminación de un mensaje para el usuario actual (eliminación lógica).
    /// </summary>
    /// <param name="messageId">Identificador único del mensaje a eliminar.</param>
    /// <returns>Una respuesta indicando el éxito de la eliminación.</returns>
    [HttpDelete("{messageId:guid}")]
    [EndpointSummary("Eliminar un mensaje")]
    [EndpointDescription("Realiza una eliminación lógica de un mensaje para el usuario actual.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMessage(Guid messageId)
    {
        var currentUserId = GetRequiredCurrentUserId();
        await messageService.DeleteMessage(messageId, currentUserId);
        return SuccessEnvelope("Mensaje eliminado correctamente");
    }
}

/// <summary>
/// Objeto de transferencia de datos (DTO) para la solicitud de envío de un mensaje directo.
/// </summary>
public class SendMessageDto
{
    /// <summary>
    /// Identificador único del usuario receptor del mensaje.
    /// </summary>
    public Guid ReceiverId { get; set; }

    /// <summary>
    /// Contenido de texto del mensaje que se va a enviar.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}
