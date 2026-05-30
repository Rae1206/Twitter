using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Interfaces.Services;
using Application.Models.DTOs.Chatbot;
using Application.Models.Requests.Chatbot;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Constants;
using Shared.Helpers;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Exceptions;
using Twitter.Domain.Interfaces;

namespace Application.Services;

public class ChatbotService(
    IUnitOfWork unitOfWork,
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<ChatbotService> logger) : IChatbotService
{
    private const string UserRole = "user";
    private const string AssistantRole = "assistant";
    private const string SystemRole = "system";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<ChatbotReplyDto> SendMessageAsync(
        Guid currentUserId,
        SendChatbotMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        await EnsureUserExistsAsync(currentUserId);

        var userMessageContent = NormalizeMessage(request.Message);
        var providerModel = GetRequiredConfiguration(ConfigurationConstants.GROQ_MODEL, "El modelo de IA no está configurado");
        var apiKey = GetRequiredConfiguration(ConfigurationConstants.GROQ_API_KEY, "La API key de Groq no está configurada");
        var recentHistory = await unitOfWork.ChatbotMessages
            .GetRecentConversationAsync(currentUserId, ChatbotConstants.RecentContextMessageLimit, cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Generando respuesta de chatbot para usuario {UserId} usando modelo {Model} con {HistoryCount} mensajes previos",
                currentUserId,
                providerModel,
                recentHistory.Count);
        }

        using var httpRequest = BuildHttpRequest(apiKey, providerModel, recentHistory, userMessageContent);
        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorDetail = payload.Length > 500 ? payload[..500] : payload;
            logger.LogWarning(
                "Groq devolvió error {StatusCode} para chatbot del usuario {UserId}. Respuesta: {Response}",
                (int)response.StatusCode,
                currentUserId,
                errorDetail);

            throw new HttpRequestException(
                $"No fue posible generar la respuesta del chatbot. Groq respondió con status {(int)response.StatusCode}: {errorDetail}",
                null,
                response.StatusCode);
        }

        var completion = DeserializeResponse(payload);
        var assistantContent = ExtractGeneratedContent(completion);
        var resolvedModel = string.IsNullOrWhiteSpace(completion.Model) ? providerModel : completion.Model;

        var createdAt = DateTimeHelper.UtcNow();
        var userMessage = new ChatbotMessage
        {
            ChatbotMessageId = Guid.NewGuid(),
            UserId = currentUserId,
            Role = UserRole,
            Content = userMessageContent,
            CreatedAt = createdAt
        };

        var assistantMessage = new ChatbotMessage
        {
            ChatbotMessageId = Guid.NewGuid(),
            UserId = currentUserId,
            Role = AssistantRole,
            Content = assistantContent,
            Model = resolvedModel,
            CreatedAt = createdAt.AddMilliseconds(1)
        };

        unitOfWork.Create(userMessage);
        unitOfWork.Create(assistantMessage);
        await unitOfWork.SaveChangesAsync();

        return new ChatbotReplyDto
        {
            UserMessage = MapToDto(userMessage),
            AssistantMessage = MapToDto(assistantMessage),
            Model = resolvedModel
        };
    }

    public async Task<IReadOnlyList<ChatbotMessageDto>> GetHistoryAsync(
        Guid currentUserId,
        int limit,
        int offset,
        CancellationToken cancellationToken = default)
    {
        await EnsureUserExistsAsync(currentUserId);

        var history = await unitOfWork.ChatbotMessages.GetHistoryAsync(currentUserId, limit, offset, cancellationToken);
        return history.Select(MapToDto).ToList();
    }

    private static HttpRequestMessage BuildHttpRequest(
        string apiKey,
        string model,
        IReadOnlyCollection<ChatbotMessage> recentHistory,
        string currentUserMessage)
    {
        var messages = new List<GroqMessage>
        {
            new()
            {
                Role = SystemRole,
                Content = BuildSystemPrompt()
            }
        };

        messages.AddRange(recentHistory.Select(historyMessage => new GroqMessage
        {
            Role = historyMessage.Role,
            Content = historyMessage.Content
        }));

        messages.Add(new GroqMessage
        {
            Role = UserRole,
            Content = currentUserMessage
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(new GroqChatCompletionRequest
        {
            Model = model,
            Messages = messages,
            MaxTokens = ChatbotConstants.MaxResponseTokens
        });

        return request;
    }

    private static string BuildSystemPrompt() =>
        "Eres un asistente útil en una red social tipo Twitter. REGLAS ESTRICTAS: 1) Responde SOLO en español. 2) Responde SOLO con el texto final, sin explicaciones, sin introducciones, sin palabras como 'Claro', '¡Por supuesto!' o similares. 3) NUNCA incluyas conteos de palabras, caracteres, ni anotaciones como (≈100 palabras). 4) NUNCA uses markdown, negritas, cursivas, ni formato especial. 5) NUNCA uses emojis numerados tipo 1️⃣ ni listas numeradas con emojis. 6) Mantén la respuesta en máximo 100 palabras. 7) Si la pregunta requiere pasos, usa texto plano simple sin emojis.";

    private static string NormalizeMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new BadRequestException("El mensaje para el chatbot es obligatorio");
        }

        var normalizedMessage = message.Trim();
        if (normalizedMessage.Length > ChatbotConstants.MaxUserMessageLength)
        {
            throw new BadRequestException($"El mensaje no puede exceder los {ChatbotConstants.MaxUserMessageLength} caracteres");
        }

        return normalizedMessage;
    }

    private async Task EnsureUserExistsAsync(Guid userId)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
        {
            throw new ResourceNotFoundException("user", userId);
        }
    }

    private string GetRequiredConfiguration(string key, string errorMessage)
    {
        var value = configuration[key];
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        logger.LogError("Configuración faltante para la clave {ConfigurationKey}", key);
        throw new InvalidOperationException(errorMessage);
    }

    private static GroqChatCompletionResponse DeserializeResponse(string payload)
    {
        try
        {
            return JsonSerializer.Deserialize<GroqChatCompletionResponse>(payload, JsonOptions)
                ?? throw new InvalidOperationException("El proveedor de IA devolvió una respuesta vacía");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("El proveedor de IA devolvió una respuesta inválida", ex);
        }
    }

    private static string ExtractGeneratedContent(GroqChatCompletionResponse completion)
    {
        var content = completion.Choices
            .FirstOrDefault()?
            .Message?
            .Content?
            .Trim();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("El proveedor de IA no devolvió contenido utilizable");
        }

        return content.Length <= ChatbotConstants.MaxAssistantResponseChars
            ? content
            : TruncateAtSentenceBoundary(content, ChatbotConstants.MaxAssistantResponseChars);
    }

    private static string TruncateAtSentenceBoundary(string text, int maxLength)
    {
        var substring = text[..maxLength];
        var lastSentenceEnd = substring.LastIndexOfAny(['.', '!', '?', '\n']);

        return lastSentenceEnd > maxLength * 0.5
            ? substring[..(lastSentenceEnd + 1)].TrimEnd()
            : substring.TrimEnd();
    }

    private static ChatbotMessageDto MapToDto(ChatbotMessage message)
    {
        return new ChatbotMessageDto
        {
            ChatbotMessageId = message.ChatbotMessageId,
            UserId = message.UserId,
            Role = message.Role,
            Content = message.Content,
            Model = message.Model,
            CreatedAt = message.CreatedAt
        };
    }

    private sealed class GroqChatCompletionRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("messages")]
        public List<GroqMessage> Messages { get; set; } = [];

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }
    }

    private sealed class GroqMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    private sealed class GroqChatCompletionResponse
    {
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("choices")]
        public List<GroqChoice> Choices { get; set; } = [];
    }

    private sealed class GroqChoice
    {
        [JsonPropertyName("message")]
        public GroqMessage? Message { get; set; }
    }
}
