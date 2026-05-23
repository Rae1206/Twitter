using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Twitter.Domain.Database.SqlServer;
using WebApi.Common;
using WebApi.Extensions;

namespace WebApi.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireNotSuspendedAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            return; // Let [Authorize] handle unauthenticated
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
