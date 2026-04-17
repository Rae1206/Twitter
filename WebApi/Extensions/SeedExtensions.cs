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
}
