using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebApi.Attributes;
using WebApi.Hubs;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[RequireNotSuspended]
public class MessageController(
    IMessageService messageService,
    IHubContext<MessageHub> hubContext) : ApiControllerBase
{
    /// <summary>
    /// Enviar un mensaje directo
    /// </summary>
    [HttpPost("send")]
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
    /// Obtener conversación con un usuario
    /// </summary>
    [HttpGet("conversation/{otherUserId:guid}")]
    public async Task<IActionResult> GetConversation(Guid otherUserId, [FromQuery] int limit = 50, [FromQuery] int offset = 0)
    {
        var currentUserId = GetRequiredCurrentUserId();
        var messages = await messageService.GetConversation(currentUserId, otherUserId, limit, offset);
        return OkEnvelope(messages);
    }

    /// <summary>
    /// Obtener lista de conversaciones (últimos mensajes con cada usuario)
    /// </summary>
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversationsList([FromQuery] int limit = 20, [FromQuery] int offset = 0)
    {
        var currentUserId = GetRequiredCurrentUserId();
        var conversations = await messageService.GetConversationsList(currentUserId, limit, offset);
        return OkEnvelope(conversations);
    }

    /// <summary>
    /// Obtener cantidad de mensajes no leídos en una conversación específica
    /// </summary>
    [HttpGet("conversation/{otherUserId:guid}/unread/count")]
    public async Task<IActionResult> GetUnreadCountInConversation(Guid otherUserId)
    {
        var currentUserId = GetRequiredCurrentUserId();
        var count = await messageService.GetUnreadCountInConversation(currentUserId, otherUserId);
        return OkEnvelope(new { count });
    }

    /// <summary>
    /// Obtener mensajes no leídos
    /// </summary>
    [HttpGet("unread")]
    public async Task<IActionResult> GetUnreadMessages()
    {
        var currentUserId = GetRequiredCurrentUserId();
        var messages = await messageService.GetUnreadMessages(currentUserId);
        return OkEnvelope(messages);
    }

    /// <summary>
    /// Obtener cantidad total de mensajes no leídos
    /// </summary>
    [HttpGet("unread/count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var currentUserId = GetRequiredCurrentUserId();
        var count = await messageService.GetUnreadCount(currentUserId);
        return OkEnvelope(new { count });
    }

    /// <summary>
    /// Marcar un mensaje como leído
    /// </summary>
    [HttpPatch("{messageId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid messageId)
    {
        var currentUserId = GetRequiredCurrentUserId();
        await messageService.MarkAsRead(messageId, currentUserId);
        return SuccessEnvelope("Mensaje marcado como leído");
    }

    /// <summary>
    /// Marcar toda la conversación como leída
    /// </summary>
    [HttpPatch("conversation/{otherUserId:guid}/read")]
    public async Task<IActionResult> MarkConversationAsRead(Guid otherUserId)
    {
        var currentUserId = GetRequiredCurrentUserId();
        await messageService.MarkConversationAsRead(currentUserId, otherUserId);
        return SuccessEnvelope("Conversación marcada como leída");
    }

    /// <summary>
    /// Eliminar un mensaje (soft delete para el usuario actual)
    /// </summary>
    [HttpDelete("{messageId:guid}")]
    public async Task<IActionResult> DeleteMessage(Guid messageId)
    {
        var currentUserId = GetRequiredCurrentUserId();
        await messageService.DeleteMessage(messageId, currentUserId);
        return SuccessEnvelope("Mensaje eliminado correctamente");
    }
}

/// <summary>
/// DTO para enviar mensajes
/// </summary>
public class SendMessageDto
{
    public Guid ReceiverId { get; set; }
    public string Content { get; set; } = string.Empty;
}
