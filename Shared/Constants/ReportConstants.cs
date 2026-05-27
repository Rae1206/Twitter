namespace Shared.Constants;

/// <summary>
/// Constantes para el módulo de reportes de contenido.
/// </summary>
public static class ReportConstants
{
    // === Estados de reporte ===
    public const string STATUS_PENDING = "pending";
    public const string STATUS_UNDER_REVIEW = "under_review";
    public const string STATUS_RESOLVED = "resolved";
    public const string STATUS_DISMISSED = "dismissed";

    // === Tipos de entidad reportable ===
    public const string ENTITY_TYPE_POST = "Post";
    public const string ENTITY_TYPE_USER = "User";
    public const string ENTITY_TYPE_MESSAGE = "Message";

    // === Categorías de reporte ===
    public const string CATEGORY_SPAM = "spam";
    public const string CATEGORY_HATE_SPEECH = "hate_speech";
    public const string CATEGORY_HARASSMENT = "harassment";
    public const string CATEGORY_MISINFORMATION = "misinformation";
    public const string CATEGORY_NUDITY = "nudity";
    public const string CATEGORY_VIOLENCE = "violence";
    public const string CATEGORY_COPYRIGHT = "copyright";
    public const string CATEGORY_OTHER = "other";

    // === Prioridades ===
    public const byte PRIORITY_HIGH = 1;
    public const byte PRIORITY_MEDIUM = 2;
    public const byte PRIORITY_LOW = 3;

    // === Mensajes de error ===
    public const string REPORT_NOT_FOUND = "El reporte no existe";
    public const string ALREADY_REPORTED = "Ya reportaste este contenido";
    public const string INVALID_ENTITY_TYPE = "Tipo de entidad no válido";
    public const string INVALID_CATEGORY = "Categoría de reporte no válida";

    // === Umbrales ===
    public const int DEFAULT_FLAG_THRESHOLD = 5;
}