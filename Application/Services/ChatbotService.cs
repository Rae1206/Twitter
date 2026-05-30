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

    /// <summary>
    /// Envía un mensaje de usuario al chatbot de IA, recupera su historial reciente, realiza la llamada a la API del proveedor de IA (Groq)
    /// y retorna la respuesta generada registrando ambos mensajes de la conversación en la base de datos.
    /// </summary>
    /// <param name="currentUserId">Identificador único del usuario remitente.</param>
    /// <param name="request">Modelo que contiene el mensaje de texto enviado.</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>La respuesta estructurada que asocia el mensaje del usuario con la respuesta del chatbot.</returns>
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

    /// <summary>
    /// Obtiene el historial de mensajes de la conversación del usuario con el chatbot de forma paginada.
    /// </summary>
    /// <param name="currentUserId">Identificador único del usuario.</param>
    /// <param name="limit">Cantidad máxima de mensajes a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <param name="cancellationToken">Token de cancelación opcional para la operación asíncrona.</param>
    /// <returns>Una lista de mensajes de la conversación representados en formato DTO.</returns>
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

    /// <summary>
    /// Construye una solicitud HTTP estructurada con las cabeceras de autorización y payload JSON para el chatbot de Groq.
    /// </summary>
    /// <param name="apiKey">La clave de API del proveedor Groq.</param>
    /// <param name="model">El modelo de IA a utilizar.</param>
    /// <param name="recentHistory">El historial de mensajes recientes para mantener el contexto de la conversación.</param>
    /// <param name="currentUserMessage">El mensaje de texto que envía el usuario actualmente.</param>
    /// <returns>Un objeto <see cref="HttpRequestMessage"/> configurado para la llamada.</returns>
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

    /// <summary>
    /// Genera las instrucciones de comportamiento de sistema (system prompt) que determinan cómo debe responder el chatbot.
    /// </summary>
    /// <returns>Las directrices de personalidad y formato para el chatbot.</returns>
    private static string BuildSystemPrompt() =>
        "Respondes en español de forma breve, como en Twitter: máximo 2 oraciones. NUNCA lists con pasos, NUNCA ingredientes, NUNCA recetas largas, NUNCA digas 'Claro' ni 'Aquí tienes'. Solo texto directo, sin markdown, sin asteriscos, sin emojis numerados, sin conteos de palabras.";

    /// <summary>
    /// Valida y normaliza el mensaje del usuario eliminando espacios y asegurando que no supere la longitud permitida.
    /// </summary>
    /// <param name="message">El mensaje sin procesar.</param>
    /// <returns>El mensaje normalizado.</returns>
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

    /// <summary>
    /// Asegura la existencia del usuario especificado en la base de datos, lanzando una excepción si no se encuentra.
    /// </summary>
    /// <param name="userId">Identificador del usuario a verificar.</param>
    private async Task EnsureUserExistsAsync(Guid userId)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId);
        if (user is null)
        {
            throw new ResourceNotFoundException("user", userId);
        }
    }

    /// <summary>
    /// Recupera y valida una clave particular de la configuración global de la aplicación.
    /// </summary>
    /// <param name="key">La clave de configuración.</param>
    /// <param name="errorMessage">Mensaje de error a incluir si la clave no tiene valor.</param>
    /// <returns>El valor de configuración recuperado.</returns>
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

    /// <summary>
    /// Deserializa la cadena JSON de respuesta desde el formato devuelto por Groq.
    /// </summary>
    /// <param name="payload">Respuesta en crudo en formato JSON.</param>
    /// <returns>El objeto <see cref="GroqChatCompletionResponse"/> deserializado.</returns>
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

    /// <summary>
    /// Extrae y normaliza el texto de respuesta generado por el chatbot en la respuesta del proveedor.
    /// </summary>
    /// <param name="completion">Respuesta estructurada de la API de Groq.</param>
    /// <returns>El texto de respuesta extraído y, opcionalmente, truncado.</returns>
    private static string ExtractGeneratedContent(GroqChatCompletionResponse completion)
    {
        var choice = completion.Choices.FirstOrDefault();
        var content = choice?.Message?.Content?.Trim();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("El proveedor de IA no devolvió contenido utilizable");
        }

        if (choice?.FinishReason == "length")
        {
            content = TruncateAtSentenceBoundary(content, content.Length);
        }

        return content.Length <= ChatbotConstants.MaxAssistantResponseChars
            ? content
            : TruncateAtSentenceBoundary(content, ChatbotConstants.MaxAssistantResponseChars);
    }

    /// <summary>
    /// Trunca de forma inteligente un texto en el último límite de oración que resulte representativo dentro de un tamaño máximo de caracteres.
    /// </summary>
    /// <param name="text">El texto original completo.</param>
    /// <param name="maxLength">Longitud máxima permitida.</param>
    /// <returns>El texto truncado en el límite de una oración si es posible.</returns>
    private static string TruncateAtSentenceBoundary(string text, int maxLength)
    {
        if (text.Length <= maxLength)
        {
            return text;
        }

        var substring = text[..maxLength];
        var lastSentenceEnd = substring.LastIndexOfAny(['.', '!', '?', '\n']);

        return lastSentenceEnd > maxLength * 0.5
            ? substring[..(lastSentenceEnd + 1)].TrimEnd()
            : substring.TrimEnd();
    }

    /// <summary>
    /// Mapea una entidad <see cref="ChatbotMessage"/> a su correspondiente DTO de representación externa.
    /// </summary>
    /// <param name="message">La entidad a mapear.</param>
    /// <returns>El DTO <see cref="ChatbotMessageDto"/> resultante.</returns>
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

    /// <summary>
    /// Estructura de solicitud JSON enviada para inicializar chat completions en la API de Groq.
    /// </summary>
    private sealed class GroqChatCompletionRequest
    {
        /// <summary>
        /// Obtiene o establece el modelo a utilizar.
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el conjunto de mensajes de la conversación.
        /// </summary>
        [JsonPropertyName("messages")]
        public List<GroqMessage> Messages { get; set; } = [];

        /// <summary>
        /// Obtiene o establece el límite máximo de tokens a generar.
        /// </summary>
        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }
    }

    /// <summary>
    /// Representa un mensaje estructurado en la conversación para la API de Groq.
    /// </summary>
    private sealed class GroqMessage
    {
        /// <summary>
        /// Obtiene o establece el rol asociado al mensaje ("system", "user" o "assistant").
        /// </summary>
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el texto de contenido del mensaje.
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// Estructura que representa la respuesta estructurada devuelta por la API de Chat Completion de Groq.
    /// </summary>
    private sealed class GroqChatCompletionResponse
    {
        /// <summary>
        /// Obtiene o establece el identificador del modelo utilizado.
        /// </summary>
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        /// <summary>
        /// Obtiene o establece el listado de opciones de respuesta generadas.
        /// </summary>
        [JsonPropertyName("choices")]
        public List<GroqChoice> Choices { get; set; } = [];
    }

    /// <summary>
    /// Representa una opción de respuesta particular en la estructura devuelta por la API de Groq.
    /// </summary>
    private sealed class GroqChoice
    {
        /// <summary>
        /// Obtiene o establece el mensaje interno de respuesta de la opción.
        /// </summary>
        [JsonPropertyName("message")]
        public GroqMessage? Message { get; set; }

        /// <summary>
        /// Obtiene o establece el motivo por el cual finalizó la generación ("stop", "length", etc.).
        /// </summary>
        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    }
}
