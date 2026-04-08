namespace Shared.Constants;

/// <summary>
/// Constantes de mensajes de error comunes 
/// </summary>
public static class ErrorConstants
{
    // === Errores de recurso no encontrado ===
    public const string RESOURCE_NOT_FOUND = "El recurso solicitado no fue encontrado";
    public const string USER_NOT_FOUND = "El usuario no existe";
    public const string POST_NOT_FOUND = "La publicación no existe";
    public const string COMMENT_NOT_FOUND = "El comentario no existe";
    public const string ROLE_NOT_FOUND = "El rol no existe";

    // === Errores de validación ===
    public const string INVALID_INPUT = "Los datos proporcionados no son válidos";
    public const string INVALID_EMAIL = "El formato del correo electrónico no es válido";
    public const string INVALID_PASSWORD = "La contraseña no cumple con los requisitos de seguridad";
    public const string INVALID_USERNAME = "El nombre de usuario no es válido";
    public const string FIELD_REQUIRED = "El campo '{0}' es obligatorio";
    public const string FIELD_TOO_LONG = "El campo '{0}' excede el máximo de {1} caracteres";
    public const string FIELD_TOO_SHORT = "El campo '{0}' debe tener al menos {1} caracteres";

    // === Errores de autenticación y autorización ===
    public const string UNAUTHORIZED = "No tiene permisos para realizar esta acción";
    public const string INVALID_CREDENTIALS = "Las credenciales proporcionadas no son válidas";
    public const string TOKEN_EXPIRED = "El token de autenticación ha expirado";
    public const string TOKEN_INVALID = "El token de autenticación no es válido";
    public const string ACCOUNT_DISABLED = "La cuenta está deshabilitada";

    // === Errores de conflicto ===
    public const string RESOURCE_CONFLICT = "El recurso ya existe";
    public const string USERNAME_TAKEN = "El nombre de usuario ya está en uso";
    public const string EMAIL_TAKEN = "El correo electrónico ya está registrado";
    public const string ALREADY_FOLLOWING = "Ya estás siguiendo a este usuario";
    public const string NOT_FOLLOWING = "No estás siguiendo a este usuario";
    public const string ALREADY_LIKED = "Ya diste like a esta publicación";
    public const string ALREADY_RETWEETED = "Ya compartiste esta publicación";

    // === Errores del servidor ===
    public const string INTERNAL_SERVER_ERROR = "Ocurrió un error interno del servidor. Intente más tarde";
    public const string UNEXPECTED_ERROR = "Ha ocurrido un error inesperado. Contacte con soporte con el siguiente código: {0}";
    public const string SERVICE_UNAVAILABLE = "El servicio no está disponible temporalmente";
    public const string EXTERNAL_SERVICE_ERROR = "Error al comunicarse con un servicio externo";

    // === Errores de negocio ===
    public const string POST_TOO_LONG = "La publicación excede el límite de caracteres permitido";
    public const string POST_EMPTY = "La publicación no puede estar vacía";
    public const string CANNOT_DELETE_OTHERS_POST = "No puede eliminar publicaciones de otros usuarios";
    public const string CANNOT_EDIT_OTHERS_POST = "No puede editar publicaciones de otros usuarios";
    public const string MAX_FOLLOWERS_REACHED = "Se alcanzó el límite máximo de seguidores";
    public const string MAX_FOLLOWING_REACHED = "Se alcanzó el límite máximo de usuarios seguidos";
}
