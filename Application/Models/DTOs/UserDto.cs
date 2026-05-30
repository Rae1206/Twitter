namespace Application.Models.DTOs;

/// <summary>
/// Objeto de transferencia de datos (DTO) que representa a un usuario en el sistema.
/// </summary>
public class UserDto
{
    /// <summary>
    /// Identificador único del usuario.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Apodo único (nickname / handler) del usuario.
    /// </summary>
    public string Nickname { get; set; } = null!;

    /// <summary>
    /// Dirección de correo electrónico del usuario.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Biografía o descripción breve del perfil del usuario.
    /// </summary>
    public string? Biography { get; set; }

    /// <summary>
    /// URL o ruta pública de acceso a la foto de perfil.
    /// </summary>
    public string? ProfilePhotoUrl { get; set; }

    /// <summary>
    /// Nombre de archivo de la foto de perfil en el almacenamiento.
    /// </summary>
    public string? ProfilePhotoFileName { get; set; }

    /// <summary>
    /// Indica si el usuario está actualmente activo en el sistema.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Indica si el usuario tiene una suspensión activa.
    /// </summary>
    public bool IsSuspended { get; set; }

    /// <summary>
    /// Indica si el usuario tiene restricciones invisibles de visibilidad (shadowbanned).
    /// </summary>
    public bool IsShadowBanned { get; set; }

    /// <summary>
    /// Fecha y hora (UTC) en la que se eliminó el usuario (si aplica).
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Cantidad total de usuarios que siguen a este usuario.
    /// </summary>
    public int FollowersCount { get; set; }

    /// <summary>
    /// Cantidad total de usuarios a los que sigue este usuario.
    /// </summary>
    public int FollowingCount { get; set; }

    /// <summary>
    /// Lista de roles de seguridad o permisos asociados al usuario.
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// Fecha y hora (UTC) de registro/creación del usuario.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
