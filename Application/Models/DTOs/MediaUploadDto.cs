using Twitter.Domain.Database.SqlServer.Entities.Enums;

namespace Application.Models.DTOs;

/// <summary>
/// Objeto de transferencia de datos (DTO) que representa un archivo multimedia subido al sistema.
/// </summary>
public class MediaUploadDto
{
    /// <summary>
    /// Identificador único del archivo multimedia.
    /// </summary>
    public Guid MediaId { get; set; }

    /// <summary>
    /// URL pública de acceso al archivo multimedia.
    /// </summary>
    public string Url { get; set; } = null!;

    /// <summary>
    /// Tipo de medio subido (por ejemplo, Imagen, Video, GIF).
    /// </summary>
    public MediaType MediaType { get; set; }
}
