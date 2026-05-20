using System.Security.Claims;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using Shared.Constants;

namespace WebApi.Filters;

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
        // No-op before action
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is not null)
        {
            return; // Don't log failed actions
        }

        if (context.Result is not Microsoft.AspNetCore.Mvc.OkObjectResult
            && context.Result is not Microsoft.AspNetCore.Mvc.NoContentResult
            && context.Result is not Microsoft.AspNetCore.Mvc.CreatedResult
            && context.Result is not Microsoft.AspNetCore.Mvc.OkResult)
        {
            return; // Only log successful actions
        }

        var user = context.HttpContext.User;
        var userIdClaim = user.FindFirst(ClaimsConstants.USER_ID)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var adminId))
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

        // Try to extract entity ID from route values
        var entityId = context.RouteData.Values["id"]?.ToString();

        _ = Task.Run(async () =>
        {
            try
            {
                await auditService.LogChangeAsync(adminId, _action, _entityType, entityId, null, null, reason: $"IP: {ip}, UA: {userAgent}");
            }
            catch
            {
                // Silently fail audit logging
            }
        });
    }
}
