namespace Shared.Constants;

/// <summary>
/// Constantes de configuración centralizadas. Permite cambiar el nombre de una configuración en un solo lugar.
/// Las claves coinciden con las propiedades en secret.json y appsettings.json.
/// </summary>
public static class ConfigurationConstants
{
    // === Connection Strings ===
    public const string CONNECTION_STRING_DATABASE = "ConnectionStrings:DefaultConnection";

    // === JWT ===
    public const string JWT_PRIVATE_KEY = "Jwt:PrivateKey";
    public const string JWT_AUDIENCE = "Jwt:Audience";
    public const string JWT_ISSUER = "Jwt:Issuer";
    public const string JWT_EXPIRATION_IN_MINUTES_MIN = "Jwt:ExpirationInMinutesMin";
    public const string JWT_EXPIRATION_IN_MINUTES_MAX = "Jwt:ExpirationInMinutesMax";

    // === Auth - Refresh Token ===
    public const string AUTH_REFRESH_TOKEN_EXPIRATION_IN_DAYS = "Auth:RefreshToken:ExpirationInDays";

    // === SMTP ===
    public const string SMTP_HOST = "SMTP:Host";
    public const string SMTP_PORT = "SMTP:Port";
    public const string SMTP_USER = "SMTP:User";
    public const string SMTP_PASSWORD = "SMTP:Password";
    public const string SMTP_FROM = "SMTP:From";
}