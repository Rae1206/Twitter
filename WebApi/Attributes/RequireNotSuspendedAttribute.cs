using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shared.Constants;
using Twitter.Domain.Database.SqlServer;

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

        var userIdClaim = user.FindFirst(ClaimsConstants.USER_ID)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return;
        }

        var unitOfWork = context.HttpContext.RequestServices.GetService<IUnitOfWork>();
        if (unitOfWork is null)
        {
            return;
        }

        var activeSuspension = unitOfWork.UserSuspensions.GetActiveSuspensionAsync(userId).Result;
        if (activeSuspension is not null)
        {
            context.Result = new ObjectResult(new { error = "Your account is suspended." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
