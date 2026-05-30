using Application.Helpers;
using Application.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApi.Common;

/// <summary>
/// Fábrica centralizada para crear respuestas API estandarizadas.
/// </summary>
public static class ApiResponseFactory
{
    /// <summary>Crea una respuesta exitosa con datos.</summary>
    public static GenericResponse<T> Success<T>(T? data, string? message = null) =>
        ResponseHelper.Success(data, message);

    /// <summary>Crea una respuesta exitosa sin datos.</summary>
    public static GenericResponse<object?> Success(string? message = null) =>
        ResponseHelper.Success(message);

    /// <summary>Crea una respuesta de error con datos opcionales.</summary>
    public static GenericResponse<T> Error<T>(string? message = null, IEnumerable<string>? errors = null, T? data = default) =>
        ResponseHelper.Error(message, errors, data);

    /// <summary>Crea una respuesta de error sin datos.</summary>
    public static GenericResponse<object?> Error(string? message = null, IEnumerable<string>? errors = null) =>
        ResponseHelper.Error(message, errors);

    /// <summary>Normaliza una respuesta existente (ajusta el mensaje de éxito).</summary>
    public static GenericResponse<T> Normalize<T>(GenericResponse<T> response, string? successMessage = null) =>
        ResponseHelper.Normalize(response, successMessage);

    public static ObjectResult BadRequest(string? message = null, IEnumerable<string>? errors = null) =>
        Build(StatusCodes.Status400BadRequest, Error(message, errors));

    public static ObjectResult Unauthorized(string? message = null, IEnumerable<string>? errors = null) =>
        Build(StatusCodes.Status401Unauthorized, Error(message, errors));

    public static ObjectResult Forbidden(string? message = null, IEnumerable<string>? errors = null) =>
        Build(StatusCodes.Status403Forbidden, Error(message, errors));

    public static ObjectResult NotFound(string? message = null, IEnumerable<string>? errors = null) =>
        Build(StatusCodes.Status404NotFound, Error(message, errors));

    public static ObjectResult Conflict(string? message = null, IEnumerable<string>? errors = null) =>
        Build(StatusCodes.Status409Conflict, Error(message, errors));

    public static ObjectResult InternalServerError(string? message = null, IEnumerable<string>? errors = null) =>
        Build(StatusCodes.Status500InternalServerError, Error(message, errors));

    /// <summary>Crea una respuesta de error de validación desde el ModelState.</summary>
    public static BadRequestObjectResult Validation(ModelStateDictionary modelState, string? message = null)
    {
        var errors = modelState.Values
            .SelectMany(value => value.Errors)
            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Solicitud inválida" : error.ErrorMessage)
            .Distinct()
            .ToList();

        return new BadRequestObjectResult(Error(message ?? "Uno o más errores de validación ocurrieron", errors));
    }

    /// <summary>Construye un ObjectResult con el código HTTP indicado.</summary>
    private static ObjectResult Build<T>(int statusCode, GenericResponse<T> response) =>
        new(response)
        {
            StatusCode = statusCode
        };
}
