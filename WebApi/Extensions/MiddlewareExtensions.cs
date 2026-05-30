using WebApi.Middlewares;

namespace WebApi.Extensions;

/// <summary>
/// Extensiones para registrar middlewares personalizados.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>Registra el middleware de manejo global de errores.</summary>
    public static IApplicationBuilder UseErrorHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ErrorHandlerMiddleware>();
    }
}
