using Application.Interfaces.Services;
using Application.Models.Requests.Chatbot;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using WebApi.Attributes;

namespace WebApi.Controllers;

[Route("api/chatbot/messages")]
[ApiController]
[Authorize]
[RequireNotSuspended]
public class ChatbotController(IChatbotService chatbotService) : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendChatbotMessageRequest request)
    {
        var currentUserId = GetRequiredCurrentUserId();
        var response = await chatbotService.SendMessageAsync(currentUserId, request, HttpContext.RequestAborted);
        return OkEnvelope(response, "Respuesta del chatbot generada correctamente");
    }

    [HttpGet]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int limit = ChatbotConstants.DefaultHistoryLimit,
        [FromQuery] int offset = 0)
    {
        var currentUserId = GetRequiredCurrentUserId();
        var response = await chatbotService.GetHistoryAsync(currentUserId, limit, offset, HttpContext.RequestAborted);
        return OkEnvelope(response);
    }
}
