using Application.Models.Responses;
using Shared.Constants;
using Shared.Exceptions;
using WebApi.Common;

namespace WebApi.Middlewares;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;

    public ErrorHandlerMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "[{TraceId}] Error no controlado | {Method} {Path} | User: {UserId} | IP: {ClientIp} | ExceptionType: {ExceptionType}",
                context.TraceIdentifier,
                context.Request.Method,
                context.Request.Path,
                context.User.Identity?.Name ?? "Anónimo",
                context.Connection.RemoteIpAddress?.ToString() ?? "Desconocida",
                ex.GetType().Name);

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;

        var response = exception switch
        {
            ResourceNotFoundException ex => Build(StatusCodes.Status404NotFound, ex.Message),
            ValidationException ex => Build(StatusCodes.Status400BadRequest, ex.Message, FlattenValidationErrors(ex)),
            ConflictException ex => Build(StatusCodes.Status409Conflict, ex.Message),
            AlreadyExistsException ex => Build(StatusCodes.Status409Conflict, ex.Message),
            ForbiddenException ex => Build(StatusCodes.Status403Forbidden, ex.Message),
            KeyNotFoundException => Build(StatusCodes.Status404NotFound, ErrorConstants.RESOURCE_NOT_FOUND),
            ArgumentException => Build(StatusCodes.Status400BadRequest, exception.Message),
            UnauthorizedAccessException => Build(StatusCodes.Status401Unauthorized, ErrorConstants.UNAUTHORIZED),
            _ => Build(StatusCodes.Status500InternalServerError, string.Format(ErrorConstants.UNEXPECTED_ERROR, traceId))
        };

        response.Body.Errors.Add($"traceId:{traceId}");
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.StatusCode;

        await context.Response.WriteAsJsonAsync(response.Body);
    }

    private static (int StatusCode, GenericResponse<object?> Body) Build(int statusCode, string message, IEnumerable<string>? errors = null) =>
        (statusCode, ApiResponseFactory.Error(message, errors));

    private static List<string> FlattenValidationErrors(ValidationException exception)
    {
        var errors = exception.Errors.Values.SelectMany(value => value).Where(value => !string.IsNullOrWhiteSpace(value)).ToList();
        return errors.Count == 0 ? [exception.Message] : errors;
    }
}
