using Application.Interfaces.Services;
using Application.Models.Requests.Chatbot;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using WebApi.Attributes;

namespace WebApi.Controllers;

/// <summary>
/// Controlador para interactuar con el chatbot de IA.
/// </summary>
[Route("api/chatbot/messages")]
[ApiController]
[Authorize]
[RequireNotSuspended]
[Tags("Chatbot")]
public class ChatbotController(IChatbotService chatbotService) : ApiControllerBase
{
    [HttpPost]
    [EndpointSummary("Enviar un mensaje al chatbot")]
    [EndpointDescription("Envía un mensaje al chatbot de IA y obtiene su respuesta en tiempo real.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendMessage([FromBody] SendChatbotMessageRequest request)
    {
        var currentUserId = GetRequiredCurrentUserId();
        var response = await chatbotService.SendMessageAsync(currentUserId, request, HttpContext.RequestAborted);
        return OkEnvelope(response, "Respuesta del chatbot generada correctamente");
    }

    [HttpGet]
    [EndpointSummary("Obtener historial del chat")]
    [EndpointDescription("Obtiene el historial paginado de la conversación del usuario actual con el chatbot.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int limit = ChatbotConstants.DefaultHistoryLimit,
        [FromQuery] int offset = 0)
    {
        var currentUserId = GetRequiredCurrentUserId();
        var response = await chatbotService.GetHistoryAsync(currentUserId, limit, offset, HttpContext.RequestAborted);
        return OkEnvelope(response);
    }
}
