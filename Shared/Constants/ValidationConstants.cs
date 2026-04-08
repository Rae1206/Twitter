namespace Shared.Constants;

/// <summary>
/// Constantes de validación 
/// </summary>
public static class ValidationConstants
{
    // === Mensajes de validación genéricos ===
    public const string REQUIRED = "El campo '{0}' es obligatorio";
    public const string MAX_LENGTH = "El campo '{0}' no puede exceder los {1} caracteres";
    public const string MIN_LENGTH = "El campo '{0}' debe tener al menos {1} caracteres";
    public const string INVALID_FORMAT = "El campo '{0}' tiene un formato inválido";
    public const string INVALID_RANGE = "El campo '{0}' debe estar entre {1} y {2}";
    public const string INVALID_VALUE = "El valor proporcionado para '{0}' no es válido";

    // === Límites de Twitter/X ===
    public const int MAX_USERNAME_LENGTH = 15;
    public const int MIN_USERNAME_LENGTH = 4;
    public const int MAX_POST_LENGTH = 280;
    public const int MAX_BIO_LENGTH = 160;
    public const int MAX_DISPLAY_NAME_LENGTH = 50;
    public const int MIN_PASSWORD_LENGTH = 8;
    public const int MAX_PASSWORD_LENGTH = 128;
    public const int MAX_EMAIL_LENGTH = 254;

    // === Expresiones regulares ===
    public const string USERNAME_PATTERN = @"^[a-zA-Z0-9_]+$";
    public const string EMAIL_PATTERN = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
}
