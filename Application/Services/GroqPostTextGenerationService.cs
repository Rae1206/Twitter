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

public class GroqPostTextGenerationService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<GroqPostTextGenerationService> logger) : IPostTextGenerationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

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

    private static string BuildSystemPrompt(int maxLength) =>
        $"Eres un generador de posts para una red social tipo Twitter. REGLAS ESTRICTAS: 1) Responde SOLO en español. 2) Responde SOLO con el texto del post, sin explicaciones, sin introducciones, sin palabras como 'Claro' o 'Aquí tienes'. 3) NUNCA incluyas conteos de palabras, caracteres, ni anotaciones como (≈100 caracteres). 4) NUNCA uses markdown, negritas, cursivas, ni formato especial. 5) NUNCA uses emojis numerados tipo 1️⃣ ni listas numeradas con emojis. 6) Mantén el resultado en máximo {maxLength} caracteres. 7) NUNCA cambies de idioma salvo que el usuario lo pida explícitamente. 8) SIEMPRE genera un post completo, incluso si la idea es vaga o sin sentido: inventa un enfoque creativo sin hacer preguntas ni pedir más contexto.";

    private static string BuildUserPrompt(string idea, string? tone, int maxLength)
    {
        var toneInstruction = string.IsNullOrWhiteSpace(tone)
            ? "Tono: natural y atractivo."
            : $"Tono: {tone.Trim()}.";

        return $"Escribe un post en español basado en esta idea. {toneInstruction} Máximo {maxLength} caracteres. Idea: {idea}";
    }

    private static string NormalizeIdea(string? idea)
    {
        if (string.IsNullOrWhiteSpace(idea))
        {
            throw new BadRequestException("La idea para generar el post es obligatoria");
        }

        return idea.Trim();
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
