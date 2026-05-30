using Application.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common;
using WebApi.Extensions;

namespace WebApi.Controllers;

/// <summary>
/// Clase base para todos los controladores de la API.
/// Provee métodos auxiliares para respuestas estandarizadas.
/// </summary>
public abstract class ApiControllerBase : ControllerBase
{
    // Respuesta exitosa con datos
    protected IActionResult OkEnvelope<T>(T data, string? message = null) =>
        Ok(ApiResponseFactory.Success(data, message));

    // Respuesta exitosa normalizando un GenericResponse
    protected IActionResult OkEnvelope<T>(GenericResponse<T> response, string? successMessage = null) =>
        Ok(ApiResponseFactory.Normalize(response, successMessage));

    // Respuesta 201 Created con ubicación del recurso
    protected IActionResult CreatedEnvelope<T>(string actionName, object? routeValues, T data, string? message = null) =>
        CreatedAtAction(actionName, routeValues, ApiResponseFactory.Success(data, message ?? "Recurso creado correctamente"));

    // Respuesta 202 Accepted con datos
    protected IActionResult AcceptedEnvelope<T>(T data, string? message = null) =>
        Ok(ApiResponseFactory.Success(data, message));

    // Respuesta exitosa sin datos
    protected IActionResult SuccessEnvelope(string? message = null) =>
        Ok(ApiResponseFactory.Success(message ?? "Solicitud realizada correctamente"));

    // Respuesta 400 Bad Request
    protected IActionResult BadRequestEnvelope(string? message = null, IEnumerable<string>? errors = null) =>
        ApiResponseFactory.BadRequest(message, errors);

    // Respuesta 404 Not Found
    protected IActionResult NotFoundEnvelope(string? message = null, IEnumerable<string>? errors = null) =>
        ApiResponseFactory.NotFound(message, errors);

    // Respuesta 409 Conflict
    protected IActionResult ConflictEnvelope(string? message = null, IEnumerable<string>? errors = null) =>
        ApiResponseFactory.Conflict(message, errors);

    // Intenta obtener el ID del usuario autenticado (puede ser null)
    protected Guid? TryGetCurrentUserId() => User.TryGetUserId();

    // Obtiene el ID del usuario autenticado (lanza excepción si no existe)
    protected Guid GetRequiredCurrentUserId() => User.GetRequiredUserId();
}
