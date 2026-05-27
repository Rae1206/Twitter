using Application.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common;
using WebApi.Extensions;

namespace WebApi.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult OkEnvelope<T>(T data, string? message = null) =>
        Ok(ApiResponseFactory.Success(data, message));

    protected IActionResult OkEnvelope<T>(GenericResponse<T> response, string? successMessage = null) =>
        Ok(ApiResponseFactory.Normalize(response, successMessage));

    protected IActionResult CreatedEnvelope<T>(string actionName, object? routeValues, T data, string? message = null) =>
        CreatedAtAction(actionName, routeValues, ApiResponseFactory.Success(data, message ?? "Recurso creado correctamente"));

    protected IActionResult AcceptedEnvelope<T>(T data, string? message = null) =>
        Ok(ApiResponseFactory.Success(data, message));

    protected IActionResult SuccessEnvelope(string? message = null) =>
        Ok(ApiResponseFactory.Success(message ?? "Solicitud realizada correctamente"));

    protected IActionResult BadRequestEnvelope(string? message = null, IEnumerable<string>? errors = null) =>
        ApiResponseFactory.BadRequest(message, errors);

    protected IActionResult NotFoundEnvelope(string? message = null, IEnumerable<string>? errors = null) =>
        ApiResponseFactory.NotFound(message, errors);

    protected IActionResult ConflictEnvelope(string? message = null, IEnumerable<string>? errors = null) =>
        ApiResponseFactory.Conflict(message, errors);

    protected Guid? TryGetCurrentUserId() => User.TryGetUserId();

    protected Guid GetRequiredCurrentUserId() => User.GetRequiredUserId();
}
