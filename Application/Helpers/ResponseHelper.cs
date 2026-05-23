using Application.Models.Responses;

namespace Application.Helpers;

/// <summary>
/// Helper estático para crear respuestas genéricas.
/// </summary>
public static class ResponseHelper
{
    private const string DefaultSuccessMessage = "Solicitud realizada correctamente";
    private const string DefaultErrorMessage = "La solicitud no pudo procesarse";

    /// <summary>
    /// Crea una respuesta genérica con datos, mensaje y errores opcionales.
    /// </summary>
    /// <typeparam name="T">Tipo de los datos</typeparam>
    /// <param name="data">Datos a retornar</param>
    /// <param name="errors">Lista de errores (opcional)</param>
    /// <param name="message">Mensaje de la respuesta (opcional)</param>
    /// <returns>GenericResponse con los datos proporcionados</returns>
    public static GenericResponse<T> Create<T>(T? data, List<string>? errors = null, string? message = null)
    {
        var normalizedErrors = errors ?? [];
        return new GenericResponse<T>
        {
            Success = normalizedErrors.Count == 0,
            Data = data,
            Message = message ?? (normalizedErrors.Count == 0 ? DefaultSuccessMessage : DefaultErrorMessage),
            Errors = normalizedErrors
        };
    }

    public static GenericResponse<T> Success<T>(T? data, string? message = null) =>
        Create(data, message: message ?? DefaultSuccessMessage);

    public static GenericResponse<object?> Success(string? message = null) =>
        Success<object?>(null, message);

    public static GenericResponse<T> Error<T>(string? message = null, IEnumerable<string>? errors = null, T? data = default)
    {
        var normalizedErrors = errors?.Where(error => !string.IsNullOrWhiteSpace(error)).ToList() ?? [];
        var resolvedMessage = message ?? normalizedErrors.FirstOrDefault() ?? DefaultErrorMessage;

        return new GenericResponse<T>
        {
            Success = false,
            Data = data,
            Message = resolvedMessage,
            Errors = normalizedErrors
        };
    }

    public static GenericResponse<object?> Error(string? message = null, IEnumerable<string>? errors = null) =>
        Error<object?>(message, errors);

    public static GenericResponse<T> Normalize<T>(GenericResponse<T> response, string? successMessage = null)
    {
        response.Errors ??= [];
        response.Success = response.Errors.Count == 0;
        response.Message = string.IsNullOrWhiteSpace(response.Message)
            ? (response.Success ? successMessage ?? DefaultSuccessMessage : response.Errors.FirstOrDefault() ?? DefaultErrorMessage)
            : response.Message;

        return response;
    }
}
