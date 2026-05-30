namespace Application.Models.DTOs;

/// <summary>
/// Objeto de transferencia de datos (DTO) que representa un comentario realizado en una publicación.
/// </summary>
public class CommentDto
{
    /// <summary>
    /// Identificador único del comentario.
    /// </summary>
    public Guid CommentId { get; set; }

    /// <summary>
    /// Identificador único de la publicación a la que pertenece el comentario.
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// Identificador único del usuario que realizó el comentario.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Apodo (nickname) del usuario que realizó el comentario.
    /// </summary>
    public string UserNickname { get; set; } = string.Empty;

    /// <summary>
    /// URL o ruta del avatar del usuario que realizó el comentario.
    /// </summary>
    public string? UserAvatar { get; set; }

    /// <summary>
    /// Nombre de usuario (username) de quien realizó el comentario.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Contenido de texto del comentario.
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// Cantidad de "Me gusta" (likes) recibidos en este comentario.
    /// </summary>
    public int LikesCount { get; set; }

    /// <summary>
    /// Fecha y hora (UTC) de creación del comentario.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}