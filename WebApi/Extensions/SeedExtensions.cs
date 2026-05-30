using Twitter.Domain.Interfaces;
using Twitter.Domain.Database.SqlServer.Entities;
using Shared.Constants;
using Shared.Helpers;
using BCrypt.Net;

namespace WebApi.Extensions;

/// <summary>
/// Extensiones para sembrar datos iniciales (roles, permisos, admin, config).
/// </summary>
public static class SeedExtensions
{
    public static void SeedDefaultAdmin(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("Seed");

        // IMPORTANTE: persistir entre seeds.
        // Si no llamamos SaveChanges entre pasos, las queries posteriores van a la DB
        // y no encuentran los Roles/Permissions recién creados (siguen solo en el ChangeTracker).
        // Eso causaba que RolePermissions y UserRoles no se generaran.
        SeedRoles(unitOfWork, logger);
        unitOfWork.SaveChangesAsync().GetAwaiter().GetResult();

        SeedPermissions(unitOfWork, logger);
        unitOfWork.SaveChangesAsync().GetAwaiter().GetResult();

        SeedSystemConfig(unitOfWork, logger);
        unitOfWork.SaveChangesAsync().GetAwaiter().GetResult();

        SeedAdminUserWithSuperAdminRole(unitOfWork, logger);
        unitOfWork.SaveChangesAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Crea o actualiza al usuario administrador por defecto y le asegura el rol SuperAdmin.
    /// Idempotente: si el admin ya existe pero le falta el rol, se lo asigna.
    /// </summary>
    private static void SeedAdminUserWithSuperAdminRole(IUnitOfWork unitOfWork, ILogger logger)
    {
        var existing = unitOfWork.Users.GetByEmailAsync(DefaultUserConstants.AdminEmail).GetAwaiter().GetResult();

        User adminUser;
        if (existing is null)
        {
            adminUser = new User
            {
                UserId = Guid.NewGuid(),
                Nickname = DefaultUserConstants.AdminNickname,
                Email = DefaultUserConstants.AdminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(DefaultUserConstants.AdminPassword),
                IsActive = true,
                CreatedAt = DateTimeHelper.UtcNow()
            };
            unitOfWork.Create(adminUser);
            logger.LogInformation("Usuario administrador creado | Email: {Email}", DefaultUserConstants.AdminEmail);
        }
        else
        {
            adminUser = existing;
            // Restaurar estado conocido en cada arranque (seed de dev/test).
            adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(DefaultUserConstants.AdminPassword);
            adminUser.IsActive = true;
            adminUser.IsSuspended = false;
            adminUser.IsShadowBanned = false;
            adminUser.DeletedAt = null;
            adminUser.DeletedByAdminId = null;
            unitOfWork.Update(adminUser);
            logger.LogInformation("Usuario administrador ya existía, se restauró el estado por defecto");
        }

        // Asegurar que el admin tiene el rol SuperAdmin (idempotente).
        var superAdminRoleId = unitOfWork.Roles.GetRoleIdByNameAsync(RoleConstants.SuperAdmin).GetAwaiter().GetResult();
        if (!superAdminRoleId.HasValue)
        {
            logger.LogError("No se encontró el rol SuperAdmin durante el seed del admin");
            return;
        }

        var alreadyAssigned = unitOfWork.Roles
            .GetRolesByUserIdAsync(adminUser.UserId)
            .GetAwaiter().GetResult()
            .Any(r => r.RoleId == superAdminRoleId.Value);

        if (!alreadyAssigned)
        {
            unitOfWork.Create(new UserRole
            {
                UserRoleId = Guid.NewGuid(),
                UserId = adminUser.UserId,
                RoleId = superAdminRoleId.Value,
                AssignedAt = DateTimeHelper.UtcNow()
            });
            logger.LogInformation("Rol SuperAdmin asignado al usuario administrador");
        }
    }

    private static void SeedRoles(IUnitOfWork unitOfWork, ILogger logger)
    {
        var roles = new[] { RoleConstants.Admin, RoleConstants.User, RoleConstants.Developer, RoleConstants.Moderator, RoleConstants.SuperAdmin };
        foreach (var roleName in roles)
        {
            var existing = unitOfWork.Roles.GetByNameAsync(roleName).GetAwaiter().GetResult();
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
            var existing = unitOfWork.Permissions.GetByFieldAsync(p => p.Name == name).GetAwaiter().GetResult();
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

        // Persistimos los permisos antes de relacionarlos para evitar el mismo problema:
        // RolePermissions.AssignAsync hace un AnyAsync que va a DB.
        unitOfWork.SaveChangesAsync().GetAwaiter().GetResult();

        // Asignar todos los permisos a los roles Admin y SuperAdmin.
        var adminRole = unitOfWork.Roles.GetByNameAsync(RoleConstants.Admin).GetAwaiter().GetResult();
        var superAdminRole = unitOfWork.Roles.GetByNameAsync(RoleConstants.SuperAdmin).GetAwaiter().GetResult();
        var moderatorRole = unitOfWork.Roles.GetByNameAsync(RoleConstants.Moderator).GetAwaiter().GetResult();

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
            ("site.name", "Twitter Clone", "string", "general", "Nombre del sitio"),
            ("site.maintenance", "false", "boolean", "general", "Modo mantenimiento"),
            ("posts.max_length", "280", "number", "posts", "Máximo de caracteres por post")
        };

        foreach (var (key, value, valueType, module, description) in configs)
        {
            var existing = unitOfWork.SystemConfigs.GetByKeyAsync(key).GetAwaiter().GetResult();
            if (existing is null)
            {
                unitOfWork.Create(new SystemConfig
                {
                    ConfigId = Guid.NewGuid(),
                    Key = key,
                    Value = value,
                    ValueType = valueType,
                    Module = module,
                    Description = description,
                    UpdatedAt = DateTimeHelper.UtcNow()
                });
                logger.LogInformation("Configuración creada: {Key}", key);
            }
        }
    }
}
