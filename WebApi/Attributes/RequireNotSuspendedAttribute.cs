using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Twitter.Domain.Interfaces;
using WebApi.Common;
using WebApi.Extensions;

namespace WebApi.Attributes;

/// <summary>
/// Filtro de autorización que bloquea el acceso si el usuario está suspendido.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireNotSuspendedAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            return; // Dejar que [Authorize] maneje los no autenticados
        }

        var userId = user.TryGetUserId();
        if (!userId.HasValue)
        {
            return;
        }

        var unitOfWork = context.HttpContext.RequestServices.GetService<IUnitOfWork>();
        if (unitOfWork is null)
        {
            return;
        }

        var activeSuspension = unitOfWork.UserSuspensions.GetActiveSuspensionAsync(userId.Value).Result;
        if (activeSuspension is not null)
        {
            context.Result = ApiResponseFactory.Forbidden("Tu cuenta está suspendida.");
        }
    }
}
