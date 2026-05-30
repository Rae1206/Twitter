namespace Application.Models.DTOs;

/// <summary>
/// Objeto de transferencia de datos (DTO) que representa un retweet (republicación) de una publicación existente.
/// </summary>
public class RetweetDto
{
    /// <summary>
    /// Identificador único del retweet.
    /// </summary>
    public Guid RetweetId { get; set; }

    /// <summary>
    /// Identificador único de la publicación original que fue retuiteada.
    /// </summary>
    public Guid OriginalPostId { get; set; }

    /// <summary>
    /// Identificador único del usuario que realizó el retweet.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Apodo (nickname) del usuario que realizó el retweet.
    /// </summary>
    public string UserNickname { get; set; } = string.Empty;

    /// <summary>
    /// URL o ruta del avatar del usuario que realizó el retweet.
    /// </summary>
    public string? UserAvatar { get; set; }

    /// <summary>
    /// Nombre de usuario (username) de quien realizó el retweet.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Comentario adjunto al retweet (si se trata de un retweet con cita).
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Cantidad total de "Me gusta" (likes) que tiene este retweet en particular.
    /// </summary>
    public int LikesCount { get; set; }

    /// <summary>
    /// Fecha y hora (UTC) de creación del retweet.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}