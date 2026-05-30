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
            logger.LogWarning(
                "Groq devolvió error {StatusCode} para usuario {UserId}. Respuesta: {Response}",
                (int)response.StatusCode,
                currentUserId,
                payload);

            throw new HttpRequestException(
                "No fue posible generar texto con IA en este momento",
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
        $"You write concise social media posts. Return only the final post text with no explanation, no surrounding quotes, and no markdown. Keep the result within {maxLength} characters.";

    private static string BuildUserPrompt(string idea, string? tone, int maxLength)
    {
        var toneInstruction = string.IsNullOrWhiteSpace(tone)
            ? "Tone: natural and engaging."
            : $"Tone: {tone.Trim()}.";

        return $"Write one post based on this idea. {toneInstruction} Maximum length: {maxLength} characters. Idea: {idea}";
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

        return content[..maxLength].TrimEnd();
    }

    private sealed class GroqChatCompletionRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("messages")]
        public List<GroqMessage> Messages { get; set; } = [];
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
