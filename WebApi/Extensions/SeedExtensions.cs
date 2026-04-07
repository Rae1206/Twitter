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
            Role = RoleConstants.Admin,
            CreatedAt = DateTimeHelper.UtcNow()
        };

        userRepository.Create(adminUser);
        logger.LogInformation("Usuario administrador creado exitosamente | Email: {Email}", DefaultUserConstants.AdminEmail);
    }
}
