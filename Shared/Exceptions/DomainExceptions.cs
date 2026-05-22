namespace Shared.Exceptions;

/// <summary>
/// Excepción lanzada cuando un recurso no es encontrado.
/// Se mapea a HTTP 404 Not Found.
/// </summary>
public class ResourceNotFoundException : Exception
{
    public string ResourceType { get; }

    public ResourceNotFoundException(string resourceType, object resourceId)
        : base($"El recurso {resourceType} con ID '{resourceId}' no fue encontrado")
    {
        ResourceType = resourceType;
    }

    public ResourceNotFoundException(string message) : base(message)
    {
        ResourceType = "Unknown";
    }
}

/// <summary>
/// Excepción lanzada cuando los datos de entrada no son válidos.
/// Se mapea a HTTP 400 Bad Request.
/// </summary>
public class ValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("Uno o más errores de validación ocurrieron")
    {
        Errors = errors;
    }

    public ValidationException(string message, Dictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors;
    }
}

/// <summary>
/// Excepción lanzada cuando hay un conflicto con el estado actual del recurso.
/// Se mapea a HTTP 409 Conflict.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}

/// <summary>
/// Excepción lanzada cuando el usuario no tiene permisos para una acción.
/// Se mapea a HTTP 403 Forbidden.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}

/// <summary>
/// Excepción lanzada cuando la entidad ya existe.
/// Se mapea a HTTP 409 Conflict.
/// </summary>
public class AlreadyExistsException : Exception
{
    public string EntityType { get; }

    public AlreadyExistsException(string entityType, string field, string value)
        : base($"Ya existe un {entityType} con {field} '{value}'")
    {
        EntityType = entityType;
    }

    public AlreadyExistsException(string message) : base(message)
    {
        EntityType = "Unknown";
    }
}

/// <summary>
/// Excepción lanzada cuando un archivo de media no pasa la validación.
/// Se mapea a HTTP 400 Bad Request.
/// </summary>
public class MediaValidationException : Exception
{
    public MediaValidationException(string message) : base(message)
    {
    }
}
