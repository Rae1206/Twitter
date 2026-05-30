namespace Application.Models.DTOs;

/// <summary>
/// Objeto de transferencia de datos (DTO) que contiene el texto de una publicación sugerida o generada por IA.
/// </summary>
public class GeneratedPostTextDto
{
    /// <summary>
    /// Contenido de texto sugerido para la publicación.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del modelo de IA que generó el texto.
    /// </summary>
    public string Model { get; set; } = string.Empty;
}
