using Twitter.Domain.Database.SqlServer;
using Twitter.Domain.Database.SqlServer.Entities;
using Shared.Constants;
using Shared.Helpers;
using BCrypt.Net;

namespace WebApi.Extensions;

public static class SeedExtensions
{
    public static void SeedDefaultAdmin(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("Seed");

        SeedRoles(unitOfWork, logger);
        SeedPermissions(unitOfWork, logger);
        SeedSystemConfig(unitOfWork, logger);

        if (unitOfWork.Users.ExistsByEmail(DefaultUserConstants.AdminEmail))
        {
            logger.LogInformation("El usuario administrador ya existe, se omite la creación");
            return;
        }

        var adminUser = new User
        {
            UserId = Guid.NewGuid(),
            FullName = DefaultUserConstants.AdminFullName,
            Email = DefaultUserConstants.AdminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(DefaultUserConstants.AdminPassword),
            IsActive = true,
            CreatedAt = DateTimeHelper.UtcNow()
        };

        unitOfWork.Create(adminUser);

        var adminRoleId = unitOfWork.Roles.GetRoleIdByName(RoleConstants.Admin);
        if (adminRoleId.HasValue)
        {
            var userRole = new UserRole
            {
                UserRoleId = Guid.NewGuid(),
                UserId = adminUser.UserId,
                RoleId = adminRoleId.Value,
                AssignedAt = DateTimeHelper.UtcNow()
            };
            
            unitOfWork.Create(userRole);
        }

        unitOfWork.SaveChangesAsync().GetAwaiter().GetResult();

        logger.LogInformation("Usuario administrador creado exitosamente | Email: {Email}", DefaultUserConstants.AdminEmail);
    }

    private static void SeedRoles(IUnitOfWork unitOfWork, ILogger logger)
    {
        var roles = new[] { RoleConstants.Admin, RoleConstants.User, RoleConstants.Developer, RoleConstants.Moderator, RoleConstants.SuperAdmin };
        foreach (var roleName in roles)
        {
            var existing = unitOfWork.Roles.GetByName(roleName);
            if (existing is null)
            {
                unitOfWork.Create(new Role
                {
                    RoleId = Guid.NewGuid(),
                    Name = roleName,
                    Description = $"Rol {roleName}",
                    IsActive = true,
                    CreatedAt = DateTimeHelper.UtcNow()
                });
                logger.LogInformation("Rol creado: {RoleName}", roleName);
            }
        }
    }

    private static void SeedPermissions(IUnitOfWork unitOfWork, ILogger logger)
    {
        var permissions = new[]
        {
            (PermissionConstants.UsersView, "users", "Ver usuarios"),
            (PermissionConstants.UsersDelete, "users", "Eliminar usuarios"),
            (PermissionConstants.UsersVerify, "users", "Verificar usuarios"),
            (PermissionConstants.UsersRoles, "users", "Gestionar roles de usuarios"),
            (PermissionConstants.UsersBan, "users", "Suspender/banear usuarios"),
            (PermissionConstants.PostsView, "posts", "Ver posts"),
            (PermissionConstants.PostsDelete, "posts", "Eliminar posts"),
            (PermissionConstants.PostsFlag, "posts", "Marcar posts"),
            (PermissionConstants.ReportsView, "reports", "Ver reportes"),
            (PermissionConstants.ReportsAssign, "reports", "Asignar reportes"),
            (PermissionConstants.ReportsResolve, "reports", "Resolver reportes"),
            (PermissionConstants.ConfigView, "config", "Ver configuración"),
            (PermissionConstants.ConfigEdit, "config", "Editar configuración"),
            (PermissionConstants.AuditView, "audit", "Ver auditoría"),
            (PermissionConstants.DashboardView, "dashboard", "Ver dashboard"),
            (PermissionConstants.SessionsView, "sessions", "Ver sesiones")
        };

        var permissionEntities = new List<Permission>();
        foreach (var (name, module, description) in permissions)
        {
            var existing = unitOfWork.Permissions.GetByField(p => p.Name == name);
            if (existing is null)
            {
                var perm = new Permission
                {
                    PermissionId = Guid.NewGuid(),
                    Name = name,
                    Module = module,
                    Description = description,
                    CreatedAt = DateTimeHelper.UtcNow()
                };
                unitOfWork.Create(perm);
                permissionEntities.Add(perm);
                logger.LogInformation("Permiso creado: {PermissionName}", name);
            }
            else
            {
                permissionEntities.Add(existing);
            }
        }

        // Assign all permissions to Admin and SuperAdmin roles
        var adminRole = unitOfWork.Roles.GetByName(RoleConstants.Admin);
        var superAdminRole = unitOfWork.Roles.GetByName(RoleConstants.SuperAdmin);
        var moderatorRole = unitOfWork.Roles.GetByName(RoleConstants.Moderator);

        if (adminRole is not null)
        {
            foreach (var perm in permissionEntities)
            {
                unitOfWork.RolePermissions.AssignAsync(adminRole.RoleId, perm.PermissionId).GetAwaiter().GetResult();
            }
            logger.LogInformation("Permisos asignados al rol Admin");
        }

        if (superAdminRole is not null)
        {
            foreach (var perm in permissionEntities)
            {
                unitOfWork.RolePermissions.AssignAsync(superAdminRole.RoleId, perm.PermissionId).GetAwaiter().GetResult();
            }
            logger.LogInformation("Permisos asignados al rol SuperAdmin");
        }

        if (moderatorRole is not null)
        {
            var moderatorPerms = new[]
            {
                PermissionConstants.UsersView,
                PermissionConstants.PostsView,
                PermissionConstants.PostsFlag,
                PermissionConstants.ReportsView,
                PermissionConstants.ReportsAssign,
                PermissionConstants.ReportsResolve,
                PermissionConstants.DashboardView
            };
            foreach (var permName in moderatorPerms)
            {
                var perm = permissionEntities.FirstOrDefault(p => p.Name == permName);
                if (perm is not null)
                {
                    unitOfWork.RolePermissions.AssignAsync(moderatorRole.RoleId, perm.PermissionId).GetAwaiter().GetResult();
                }
            }
            logger.LogInformation("Permisos asignados al rol Moderator");
        }
    }

    private static void SeedSystemConfig(IUnitOfWork unitOfWork, ILogger logger)
    {
        var configs = new[]
        {
            ("site.name", "Twitter Clone", "Nombre del sitio", true),
            ("site.maintenance", "false", "Modo mantenimiento", true),
            ("posts.max_length", "280", "Máximo de caracteres por post", true)
        };

        foreach (var (key, value, description, editable) in configs)
        {
            var existing = unitOfWork.SystemConfigs.GetByKeyAsync(key).GetAwaiter().GetResult();
            if (existing is null)
            {
                unitOfWork.Create(new SystemConfig
                {
                    ConfigId = Guid.NewGuid(),
                    Key = key,
                    Value = value,
                    Description = description,
                    IsEditable = editable,
                    CreatedAt = DateTimeHelper.UtcNow()
                });
                logger.LogInformation("Configuración creada: {Key}", key);
            }
        }
    }
}
