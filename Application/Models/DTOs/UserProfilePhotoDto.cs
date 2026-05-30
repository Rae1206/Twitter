namespace Application.Models.DTOs;

/// <summary>
/// Objeto de transferencia de datos (DTO) que representa los detalles de la foto de perfil de un usuario.
/// </summary>
public class UserProfilePhotoDto
{
    /// <summary>
    /// Nombre del archivo de la foto de perfil guardado.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Ruta o directorio del almacenamiento donde se encuentra el archivo físico.
    /// </summary>
    public string? StoragePath { get; set; }

    /// <summary>
    /// URL pública para la visualización o descarga de la foto de perfil.
    /// </summary>
    public string? Url { get; set; }
}
