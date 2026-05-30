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
    /// <summary>
    /// Devuelve una respuesta exitosa (HTTP 200 OK) envolviendo los datos provistos.
    /// </summary>
    /// <typeparam name="T">Tipo de los datos de la respuesta.</typeparam>
    /// <param name="data">Los datos a enviar en el cuerpo de la respuesta.</param>
    /// <param name="message">Un mensaje opcional de éxito.</param>
    /// <returns>Un objeto <see cref="IActionResult"/> que contiene el sobre de respuesta.</returns>
    protected IActionResult OkEnvelope<T>(T data, string? message = null) =>
        Ok(ApiResponseFactory.Success(data, message));

    /// <summary>
    /// Devuelve una respuesta exitosa (HTTP 200 OK) normalizando una respuesta genérica del sistema.
    /// </summary>
    /// <typeparam name="T">Tipo de los datos envueltos en la respuesta genérica.</typeparam>
    /// <param name="response">La respuesta genérica a normalizar.</param>
    /// <param name="successMessage">Un mensaje de éxito opcional para la respuesta.</param>
    /// <returns>Un objeto <see cref="IActionResult"/> con el formato normalizado.</returns>
    protected IActionResult OkEnvelope<T>(GenericResponse<T> response, string? successMessage = null) =>
        Ok(ApiResponseFactory.Normalize(response, successMessage));

    /// <summary>
    /// Devuelve una respuesta HTTP 201 Created con la ubicación del recurso y los datos creados.
    /// </summary>
    /// <typeparam name="T">Tipo del recurso creado.</typeparam>
    /// <param name="actionName">El nombre de la acción/método para obtener el recurso.</param>
    /// <param name="routeValues">Los valores de ruta necesarios para generar la URL del recurso.</param>
    /// <param name="data">El recurso creado.</param>
    /// <param name="message">Un mensaje opcional de éxito.</param>
    /// <returns>Un objeto <see cref="IActionResult"/> que representa la creación del recurso.</returns>
    protected IActionResult CreatedEnvelope<T>(string actionName, object? routeValues, T data, string? message = null) =>
        CreatedAtAction(actionName, routeValues, ApiResponseFactory.Success(data, message ?? "Recurso creado correctamente"));

    /// <summary>
    /// Devuelve una respuesta HTTP 202 Accepted indicando que la solicitud fue aceptada con datos.
    /// </summary>
    /// <typeparam name="T">Tipo de los datos de la respuesta.</typeparam>
    /// <param name="data">Los datos de la respuesta.</param>
    /// <param name="message">Un mensaje opcional indicando el estado de la solicitud.</param>
    /// <returns>Un objeto <see cref="IActionResult"/> con el resultado aceptado.</returns>
    protected IActionResult AcceptedEnvelope<T>(T data, string? message = null) =>
        Ok(ApiResponseFactory.Success(data, message));

    /// <summary>
    /// Devuelve una respuesta exitosa (HTTP 200 OK) sin datos adjuntos.
    /// </summary>
    /// <param name="message">Un mensaje opcional de éxito.</param>
    /// <returns>Un objeto <see cref="IActionResult"/> que representa el éxito de la operación.</returns>
    protected IActionResult SuccessEnvelope(string? message = null) =>
        Ok(ApiResponseFactory.Success(message ?? "Solicitud realizada correctamente"));

    /// <summary>
    /// Devuelve una respuesta HTTP 400 Bad Request debido a datos incorrectos o errores de validación.
    /// </summary>
    /// <param name="message">Un mensaje explicativo del error.</param>
    /// <param name="errors">Una lista opcional con detalles específicos de los errores.</param>
    /// <returns>Un objeto <see cref="IActionResult"/> con el error 400 estructurado.</returns>
    protected IActionResult BadRequestEnvelope(string? message = null, IEnumerable<string>? errors = null) =>
        ApiResponseFactory.BadRequest(message, errors);

    /// <summary>
    /// Devuelve una respuesta HTTP 404 Not Found cuando un recurso solicitado no existe.
    /// </summary>
    /// <param name="message">Un mensaje opcional explicando qué recurso no fue encontrado.</param>
    /// <param name="errors">Una lista opcional con detalles adicionales del error.</param>
    /// <returns>Un objeto <see cref="IActionResult"/> con el error 404 estructurado.</returns>
    protected IActionResult NotFoundEnvelope(string? message = null, IEnumerable<string>? errors = null) =>
        ApiResponseFactory.NotFound(message, errors);

    /// <summary>
    /// Devuelve una respuesta HTTP 409 Conflict cuando ocurre un conflicto con el estado actual del recurso.
    /// </summary>
    /// <param name="message">Un mensaje de conflicto.</param>
    /// <param name="errors">Una lista opcional con detalles adicionales sobre el conflicto.</param>
    /// <returns>Un objeto <see cref="IActionResult"/> con el error 409 estructurado.</returns>
    protected IActionResult ConflictEnvelope(string? message = null, IEnumerable<string>? errors = null) =>
        ApiResponseFactory.Conflict(message, errors);

    /// <summary>
    /// Intenta obtener el identificador único (Guid) del usuario actualmente autenticado (puede devolver null si no lo está).
    /// </summary>
    /// <returns>El identificador del usuario autenticado o null.</returns>
    protected Guid? TryGetCurrentUserId() => User.TryGetUserId();

    /// <summary>
    /// Obtiene obligatoriamente el identificador único (Guid) del usuario actualmente autenticado. Lanza excepción si no se encuentra.
    /// </summary>
    /// <returns>El identificador único del usuario autenticado.</returns>
    protected Guid GetRequiredCurrentUserId() => User.GetRequiredUserId();
}
