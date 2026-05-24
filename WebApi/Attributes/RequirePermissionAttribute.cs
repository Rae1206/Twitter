using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shared.Constants;
using Twitter.Domain.Interfaces;
using WebApi.Common;
using WebApi.Extensions;

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
            context.Result = ApiResponseFactory.Unauthorized("Debe autenticarse para acceder a este recurso");
            return;
        }

        var userId = user.TryGetUserId();
        if (!userId.HasValue)
        {
            context.Result = ApiResponseFactory.Unauthorized("No se pudo resolver el usuario autenticado");
            return;
        }

        var cacheService = context.HttpContext.RequestServices.GetService<ICacheService>();
        if (cacheService is null)
        {
            context.Result = ApiResponseFactory.InternalServerError("No se pudo validar los permisos del usuario");
            return;
        }

        var cacheKey = $"perm:{userId.Value}";
        var permissions = cacheService.Get<List<string>>(cacheKey);

        if (permissions is null)
        {
            var unitOfWork = context.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
            var roleIds = unitOfWork.Roles.GetRolesByUserIdAsync(userId.Value).GetAwaiter().GetResult().Select(r => r.RoleId).ToList();
            var perms = new List<string>();
            foreach (var roleId in roleIds)
            {
                var rolePerms = unitOfWork.RolePermissions.GetByRoleIdAsync(roleId).GetAwaiter().GetResult();
                perms.AddRange(rolePerms.Select(rp => rp.Permission.Name));
            }
            permissions = perms.Distinct().ToList();
            cacheService.Create(cacheKey, TimeSpan.FromMinutes(5), permissions);
        }

        if (!permissions.Contains(_permission))
        {
            context.Result = ApiResponseFactory.Forbidden($"El permiso '{_permission}' es requerido.");
        }
    }
}
