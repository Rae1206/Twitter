using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Shared.Constants;
using Shared.Helpers;

namespace WebApi.Extensions;

public static class SeedExtensions
{
    public static void SeedDefaultAdmin(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TwitterDbContext>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("Seed");

        if (userRepository.ExistsByEmail(DefaultUserConstants.AdminEmail))
        {
            logger.LogInformation("El usuario administrador ya existe, se omite la creación");
            return;
        }

        var adminUser = new User
        {
            UserId = Guid.NewGuid(),
            FullName = DefaultUserConstants.AdminFullName,
            Email = DefaultUserConstants.AdminEmail,
            PasswordHash = DefaultUserConstants.AdminPassword,
            IsActive = true,
            CreatedAt = DateTimeHelper.UtcNow()
        };

        userRepository.Create(adminUser);

        // Asignar rol de Admin
        var adminRoleId = roleRepository.GetRoleIdByName(RoleConstants.Admin);
        if (adminRoleId.HasValue)
        {
            var userRole = new UserRole
            {
                UserRoleId = Guid.NewGuid(),
                UserId = adminUser.UserId,
                RoleId = adminRoleId.Value,
                AssignedAt = DateTimeHelper.UtcNow()
            };
            
            dbContext.UserRoles.Add(userRole);
            dbContext.SaveChanges();
        }

        logger.LogInformation("Usuario administrador creado exitosamente | Email: {Email}", DefaultUserConstants.AdminEmail);
    }
}