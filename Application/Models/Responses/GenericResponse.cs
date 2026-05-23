using Shared.Helpers;

namespace Application.Models.Responses;

/// <summary>
/// Respuesta genérica estándar para todos los endpoints.
/// </summary>
/// <typeparam name="T">Tipo de datos retornados</typeparam>
public class GenericResponse<T>
{
    public bool Success { get; set; } = true;
    public T? Data { get; set; }
    public string Message { get; set; } = "Solicitud realizada correctamente";
    public List<string> Errors { get; set; } = [];
    public DateTime TimeStamp { get; } = DateTimeHelper.UtcNow();
}
