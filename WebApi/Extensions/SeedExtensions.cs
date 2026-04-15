using Domain.Context;
using Domain.Entities;
using Domain.Repositories;
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
            using var context = new TwitterDbContext();
            var userRole = new UserRole
            {
                UserRoleId = Guid.NewGuid(),
                UserId = adminUser.UserId,
                RoleId = adminRoleId.Value,
                AssignedAt = DateTimeHelper.UtcNow()
            };
            
            context.UserRoles.Add(userRole);
            context.SaveChanges();
        }

        logger.LogInformation("Usuario administrador creado exitosamente | Email: {Email}", DefaultUserConstants.AdminEmail);
    }
}
