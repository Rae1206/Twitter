using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Interfaces.Services;
using Application.Models.DTOs;
using Application.Models.Requests.Post;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Constants;
using Twitter.Domain.Exceptions;

namespace Application.Services;

/// <summary>
/// Servicio encargado de la generación de texto sugerido para publicaciones usando Inteligencia Artificial a través del proveedor Groq.
/// </summary>
public class GroqPostTextGenerationService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<GroqPostTextGenerationService> logger) : IPostTextGenerationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Genera de forma asíncrona un texto sugerido para una publicación en base a una idea y tono especificados.
    /// Valida los parámetros de entrada y realiza la llamada HTTP hacia la API de Groq.
    /// </summary>
    /// <param name="currentUserId">Identificador único del usuario solicitante.</param>
    /// <param name="model">Modelo de solicitud con la idea, tono y longitud máxima del post.</param>
    /// <param name="cancellationToken">Token opcional de cancelación de la operación asíncrona.</param>
    /// <returns>El DTO conteniendo el contenido de texto generado por la IA y el modelo utilizado.</returns>
    public async Task<GeneratedPostTextDto> GenerateAsync(
        Guid currentUserId,
        GeneratePostTextRequest model,
        CancellationToken cancellationToken = default)
    {
        var idea = NormalizeIdea(model.Idea);
        var maxLength = model.MaxLength ?? ValidationConstants.MAX_POST_LENGTH;
        var providerModel = GetRequiredConfiguration(ConfigurationConstants.GROQ_MODEL, "El modelo de IA no está configurado");
        var apiKey = GetRequiredConfiguration(ConfigurationConstants.GROQ_API_KEY, "La API key de Groq no está configurada");

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Generando texto de post con IA para usuario {UserId} usando modelo {Model}",
                currentUserId,
                providerModel);
        }

        using var request = BuildHttpRequest(apiKey, providerModel, idea, model.Tone, maxLength);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorDetail = payload.Length > 500 ? payload[..500] : payload;
            logger.LogWarning(
                "Groq devolvió error {StatusCode} para usuario {UserId}. Respuesta: {Response}",
                (int)response.StatusCode,
                currentUserId,
                errorDetail);

            throw new HttpRequestException(
                $"No fue posible generar texto con IA. Groq respondió con status {(int)response.StatusCode}: {errorDetail}",
                null,
                response.StatusCode);
        }

        var completion = DeserializeResponse(payload);
        var content = ExtractGeneratedContent(completion, maxLength);

        return new GeneratedPostTextDto
        {
            Content = content,
            Model = string.IsNullOrWhiteSpace(completion.Model) ? providerModel : completion.Model
        };
    }

    /// <summary>
    /// Construye una petición HTTP estructurada con las cabeceras de autorización y payload JSON para el endpoint de Groq Chat Completions.
    /// </summary>
    /// <param name="apiKey">La clave de API del proveedor Groq.</param>
    /// <param name="model">El modelo de IA a utilizar.</param>
    /// <param name="idea">La idea conceptual provista por el usuario.</param>
    /// <param name="tone">El tono solicitado para la publicación.</param>
    /// <param name="maxLength">Longitud máxima permitida de caracteres para el post.</param>
    /// <returns>Un objeto <see cref="HttpRequestMessage"/> inicializado.</returns>
    private static HttpRequestMessage BuildHttpRequest(
        string apiKey,
        string model,
        string idea,
        string? tone,
        int maxLength)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = JsonContent.Create(new GroqChatCompletionRequest
        {
            Model = model,
            MaxTokens = 500,
            Messages =
            [
                new GroqMessage
                {
                    Role = "system",
                    Content = BuildSystemPrompt(maxLength)
                },
                new GroqMessage
                {
                    Role = "user",
                    Content = BuildUserPrompt(idea, tone, maxLength)
                }
            ]
        });

        return request;
    }

    /// <summary>
    /// Crea el prompt de sistema (instrucciones primordiales) para guiar el comportamiento y formato de respuesta de la IA.
    /// </summary>
    /// <param name="maxLength">Longitud máxima de caracteres.</param>
    /// <returns>Las directrices de comportamiento en formato string.</returns>
    private static string BuildSystemPrompt(int maxLength) =>
        $"Eres un generador de posts para una red social tipo Twitter. REGLAS ESTRICTAS: 1) Responde SOLO en español. 2) Responde SOLO con el texto del post, sin explicaciones, sin introducciones, sin palabras como 'Claro' o 'Aquí tienes'. 3) NUNCA incluyas conteos de palabras, caracteres, ni anotaciones como (≈100 caracteres). 4) NUNCA uses markdown, negritas, cursivas, ni formato especial. 5) NUNCA uses emojis numerados tipo 1️⃣ ni listas numeradas con emojis. 6) Mantén el resultado en máximo {maxLength} caracteres. 7) NUNCA cambies de idioma salvo que el usuario lo pida explícitamente. 8) SIEMPRE genera un post completo, incluso si la idea es vaga o sin sentido: inventa un enfoque creativo sin hacer preguntas ni pedir más contexto.";

    /// <summary>
    /// Crea el prompt de usuario inyectando la idea conceptual del post y las instrucciones de tono solicitadas.
    /// </summary>
    /// <param name="idea">Idea conceptual.</param>
    /// <param name="tone">Tono deseado.</param>
    /// <param name="maxLength">Longitud máxima.</param>
    /// <returns>El prompt de usuario formateado.</returns>
    private static string BuildUserPrompt(string idea, string? tone, int maxLength)
    {
        var toneInstruction = string.IsNullOrWhiteSpace(tone)
            ? "Tono: natural y atractivo."
            : $"Tono: {tone.Trim()}.";

        return $"Escribe un post en español basado en esta idea. {toneInstruction} Máximo {maxLength} caracteres. Idea: {idea}";
    }

    /// <summary>
    /// Normaliza y valida la idea de texto de la publicación para asegurarse que no sea nula ni contenga únicamente espacios.
    /// </summary>
    /// <param name="idea">Idea a validar.</param>
    /// <returns>La idea recortada y normalizada.</returns>
    private static string NormalizeIdea(string? idea)
    {
        if (string.IsNullOrWhiteSpace(idea))
        {
            throw new BadRequestException("La idea para generar el post es obligatoria");
        }

        return idea.Trim();
    }

    /// <summary>
    /// Obtiene y valida una clave específica desde la configuración global de la aplicación.
    /// </summary>
    /// <param name="key">Nombre de la clave de configuración.</param>
    /// <param name="errorMessage">Mensaje de excepción en caso de que no esté configurada.</param>
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
    /// Deserializa la cadena JSON de respuesta obtenida desde la API de Groq.
    /// </summary>
    /// <param name="payload">Payload de respuesta JSON.</param>
    /// <returns>La respuesta deserializada en un objeto fuertemente tipado.</returns>
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
    /// Extrae el contenido de texto generado por la IA en la respuesta del proveedor y aplica reglas de truncado si sobrepasa la longitud permitida.
    /// </summary>
    /// <param name="completion">El objeto de respuesta del proveedor de IA.</param>
    /// <param name="maxLength">Longitud máxima de caracteres permitida.</param>
    /// <returns>El contenido de texto final sugerido para el post.</returns>
    private static string ExtractGeneratedContent(GroqChatCompletionResponse completion, int maxLength)
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

        if (content.Length <= maxLength)
        {
            return content;
        }

        var lastSentenceEnd = content[..maxLength].LastIndexOfAny(['.', '!', '?', '\n']);
        return lastSentenceEnd > maxLength * 0.5
            ? content[..(lastSentenceEnd + 1)].TrimEnd()
            : content[..maxLength].TrimEnd();
    }

    /// <summary>
    /// Estructura de solicitud para el endpoint de Chat Completion de Groq.
    /// </summary>
    private sealed class GroqChatCompletionRequest
    {
        /// <summary>
        /// Obtiene o establece el modelo de IA a utilizar.
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece la lista de mensajes de la conversación.
        /// </summary>
        [JsonPropertyName("messages")]
        public List<GroqMessage> Messages { get; set; } = [];

        /// <summary>
        /// Obtiene o establece la cantidad máxima de tokens a generar.
        /// </summary>
        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }
    }

    /// <summary>
    /// Representa un mensaje individual dentro de la conversación con el formato requerido por la API de Groq.
    /// </summary>
    private sealed class GroqMessage
    {
        /// <summary>
        /// Obtiene o establece el rol del emisor del mensaje (ej. "system", "user", "assistant").
        /// </summary>
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Obtiene o establece el contenido de texto del mensaje.
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// Estructura de respuesta devuelta por el endpoint de Chat Completion de Groq.
    /// </summary>
    private sealed class GroqChatCompletionResponse
    {
        /// <summary>
        /// Obtiene o establece el identificador del modelo que realmente procesó la solicitud.
        /// </summary>
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        /// <summary>
        /// Obtiene o establece las diferentes opciones de respuesta generadas por el modelo de IA.
        /// </summary>
        [JsonPropertyName("choices")]
        public List<GroqChoice> Choices { get; set; } = [];
    }

    /// <summary>
    /// Representa una opción de respuesta individual dentro de la lista de resultados de Groq.
    /// </summary>
    private sealed class GroqChoice
    {
        /// <summary>
        /// Obtiene o establece el mensaje asociado a esta opción de respuesta.
        /// </summary>
        [JsonPropertyName("message")]
        public GroqMessage? Message { get; set; }
    }
}
