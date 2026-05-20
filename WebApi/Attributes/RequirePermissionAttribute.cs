using System.Security.Claims;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shared.Constants;
using Twitter.Domain.Database.SqlServer;

namespace WebApi.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _permission;

    public RequirePermissionAttribute(string permission)
    {
        _permission = permission;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new ForbidResult();
            return;
        }

        var userIdClaim = user.FindFirst(ClaimsConstants.USER_ID)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            context.Result = new ForbidResult();
            return;
        }

        var cacheService = context.HttpContext.RequestServices.GetService<ICacheService>();
        if (cacheService is null)
        {
            context.Result = new ForbidResult();
            return;
        }

        var cacheKey = $"perm:{userId}";
        var permissions = cacheService.Get<List<string>>(cacheKey);

        if (permissions is null)
        {
            // Fallback: resolve permissions from repository
            var unitOfWork = context.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
            var roleIds = unitOfWork.Roles.GetRolesByUserId(userId).Select(r => r.RoleId).ToList();
            var perms = new List<string>();
            foreach (var roleId in roleIds)
            {
                var rolePerms = unitOfWork.RolePermissions.GetByRoleIdAsync(roleId).Result;
                perms.AddRange(rolePerms.Select(rp => rp.Permission.Name));
            }
            permissions = perms.Distinct().ToList();
            cacheService.Create(cacheKey, TimeSpan.FromMinutes(5), permissions);
        }

        if (!permissions.Contains(_permission))
        {
            context.Result = new ObjectResult(new { error = $"Permission '{_permission}' is required." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
