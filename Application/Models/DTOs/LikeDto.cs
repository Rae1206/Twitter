namespace Application.Models.DTOs;

/// <summary>
/// Objeto de transferencia de datos (DTO) que representa una reacción "Me gusta" (like) de un usuario en una publicación.
/// </summary>
public class LikeDto
{
    /// <summary>
    /// Identificador único del "Me gusta".
    /// </summary>
    public Guid LikeId { get; set; }

    /// <summary>
    /// Identificador único del usuario que dio "Me gusta".
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Identificador único de la publicación asociada con el "Me gusta".
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// Nombre de usuario (username) de quien dio "Me gusta".
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Apodo (nickname) del usuario que dio "Me gusta".
    /// </summary>
    public string UserNickname { get; set; } = string.Empty;

    /// <summary>
    /// URL o ruta del avatar del usuario que dio "Me gusta".
    /// </summary>
    public string? UserAvatar { get; set; }

    /// <summary>
    /// Fecha y hora (UTC) en la que se dio "Me gusta".
    /// </summary>
    public DateTime CreatedAt { get; set; }
}