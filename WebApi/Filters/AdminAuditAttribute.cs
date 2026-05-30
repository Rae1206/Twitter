using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using WebApi.Extensions;

namespace WebApi.Filters;

/// <summary>
/// Filtro que registra acciones administrativas en el log de auditoría.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class AdminAuditAttribute : Attribute, IActionFilter
{
    private readonly string _action;
    private readonly string _entityType;

    public AdminAuditAttribute(string action, string entityType)
    {
        _action = action;
        _entityType = entityType;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Sin acción antes de ejecutar
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is not null)
        {
            return; // No registrar acciones fallidas
        }

        if (!IsSuccessfulResult(context.Result))
        {
            return; // Solo registrar acciones exitosas
        }

        var adminId = context.HttpContext.User.TryGetUserId();
        if (!adminId.HasValue)
        {
            return;
        }

        var auditService = context.HttpContext.RequestServices.GetService<IAuditService>();
        if (auditService is null)
        {
            return;
        }

        var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.HttpContext.Request.Headers.UserAgent.ToString();

        // Intentar extraer el ID de la entidad desde los valores de ruta
        var entityId = context.RouteData.Values["id"]?.ToString();

        _ = Task.Run(async () =>
        {
            try
            {
                await auditService.LogChangeAsync(adminId.Value, _action, _entityType, entityId, null, null, reason: $"IP: {ip}, UA: {userAgent}");
            }
            catch
            {
                // Fallar silenciosamente el registro de auditoría
            }
        });
    }

    private static bool IsSuccessfulResult(object? result)
    {
        return result switch
        {
            IStatusCodeActionResult { StatusCode: >= 200 and < 300 } => true,
            null => false,
            _ => false
        };
    }
}
