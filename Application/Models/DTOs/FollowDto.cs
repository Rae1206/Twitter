namespace Application.Models.DTOs;

/// <summary>
/// Objeto de transferencia de datos (DTO) que representa la relación de seguimiento entre dos usuarios.
/// </summary>
public class FollowDto
{
    /// <summary>
    /// Identificador único del registro de seguimiento.
    /// </summary>
    public Guid FollowId { get; set; }

    /// <summary>
    /// Identificador único del usuario seguidor (el que sigue).
    /// </summary>
    public Guid FollowerId { get; set; }

    /// <summary>
    /// Identificador único del usuario seguido (al que están siguiendo).
    /// </summary>
    public Guid FollowingId { get; set; }

    /// <summary>
    /// Nombre de usuario (username) del usuario seguidor.
    /// </summary>
    public string FollowerUsername { get; set; } = string.Empty;

    /// <summary>
    /// Apodo (nickname) del usuario seguidor.
    /// </summary>
    public string FollowerNickname { get; set; } = string.Empty;

    /// <summary>
    /// URL o ruta del avatar del usuario seguidor.
    /// </summary>
    public string? FollowerAvatar { get; set; }

    /// <summary>
    /// Fecha y hora (UTC) en la que se estableció el seguimiento.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}