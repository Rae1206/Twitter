namespace Application.Models.DTOs;

/// <summary>
/// Objeto de transferencia de datos (DTO) que representa una publicación (post) en el sistema.
/// </summary>
public class PostDto
{
    /// <summary>
    /// Identificador único del post.
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// Identificador único del usuario creador de la publicación.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Apodo (nickname) del usuario creador.
    /// </summary>
    public string UserNickname { get; set; } = string.Empty;

    /// <summary>
    /// URL o ruta del avatar del usuario creador.
    /// </summary>
    public string? UserAvatar { get; set; }

    /// <summary>
    /// Nombre de usuario (username) del usuario creador.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Contenido de texto de la publicación.
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// Identificador único de la publicación a la que responde este post (si es una respuesta).
    /// </summary>
    public Guid? RepliedToPostId { get; set; }

    /// <summary>
    /// Identificador único de la publicación original (si este post es un retweet).
    /// </summary>
    public Guid? RetweetOfPostId { get; set; }

    /// <summary>
    /// Indica si la publicación está publicada y visible públicamente.
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// Cantidad de reportes acumulados por la publicación.
    /// </summary>
    public int ReportCount { get; set; }

    /// <summary>
    /// Indica si la publicación ha sido marcada o penalizada por moderación.
    /// </summary>
    public bool IsFlagged { get; set; }

    /// <summary>
    /// Motivo detallado por el cual la publicación fue eliminada (si aplica).
    /// </summary>
    public string? DeletedReason { get; set; }

    /// <summary>
    /// Cantidad total de "Me gusta" (likes) recibidos.
    /// </summary>
    public int LikesCount { get; set; }

    /// <summary>
    /// Cantidad total de veces que fue retuiteado.
    /// </summary>
    public int RetweetsCount { get; set; }

    /// <summary>
    /// Cantidad total de respuestas (comentarios) recibidas.
    /// </summary>
    public int RepliesCount { get; set; }

    /// <summary>
    /// Lista de URLs correspondientes a los archivos multimedia adjuntos a la publicación.
    /// </summary>
    public List<string>? MediaUrls { get; set; }

    /// <summary>
    /// Fecha y hora (UTC) de creación de la publicación.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha y hora (UTC) de expiración del post efímero. Si es null, el post no expira.
    /// El frontend puede usar esto para mostrar un temporizador de cuenta regresiva.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}