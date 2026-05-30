using Application.Interfaces.Services;
using Application.Models.DTOs;
using Application.Models.Requests.User;
using Application.Models.Responses;
using Twitter.Domain.Interfaces;
using Twitter.Domain.Database.SqlServer.Entities;
using Microsoft.Extensions.Logging;
using Shared.Constants;
using Twitter.Domain.Exceptions;
using Shared.Helpers;
using BCrypt.Net;

namespace Application.Services;

/// <summary>
/// Servicio encargado de la gestión integral de cuentas de usuarios, cubriendo la creación, actualización del perfil, consulta, cambio de contraseñas y eliminación/restauración lógica.
/// </summary>
public class UserService(
    IUnitOfWork unitOfWork,
    IEmailService emailService,
    ILogger<UserService> logger) : IUserService
{

    /// <summary>
    /// Crea y registra de forma asíncrona un nuevo usuario en el sistema.
    /// Valida que el correo no esté registrado previamente, genera el hash seguro para la contraseña con BCrypt,
    /// le asigna el rol inicial predeterminado de usuario y envía el correo de bienvenida correspondiente.
    /// </summary>
    /// <param name="model">Modelo conteniendo el nickname, correo y contraseña del nuevo usuario.</param>
    /// <returns>La representación DTO <see cref="UserDto"/> del usuario creado.</returns>
    public async Task<UserDto> Create(CreateUserRequest model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando crear usuario con email: {Email}", model.Email);
        }

        if (await unitOfWork.Users.ExistsByEmailAsync(model.Email))
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Intento de registro con email duplicado: {Email}", model.Email);
            }
            throw new AlreadyExistsException("usuario", "email", model.Email);
        }

        var defaultRoleId = await unitOfWork.Roles.GetRoleIdByNameAsync(RoleConstants.DefaultRole);

        if (!defaultRoleId.HasValue)
        {
            logger.LogError("No se encontró el rol por defecto: {Role}", RoleConstants.DefaultRole);
            throw new InvalidOperationException("No se pudo asignar el rol por defecto al usuario");
        }

        var entity = new User
        {
            UserId = Guid.NewGuid(),
            Nickname = model.Nickname,
            Email = model.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            IsActive = true,
            CreatedAt = DateTimeHelper.UtcNow()
        };

        unitOfWork.Create(entity);

        // Asignar el rol por defecto
        AssignRoleToUser(entity.UserId, defaultRoleId.Value);

        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Usuario creado exitosamente con ID: {UserId}", entity.UserId);
        }

        await emailService.SendWelcomeEmailAsync(entity.Email, entity.Nickname);

        return MapToDto(entity);
    }

    /// <summary>
    /// Actualiza de forma asíncrona la información básica de perfil (nickname, correo y biografía) del usuario actual.
    /// Valida que el nuevo correo electrónico no se encuentre en uso por otra cuenta activa.
    /// </summary>
    /// <param name="userId">Identificador único del usuario a actualizar.</param>
    /// <param name="model">Modelo de solicitud con los campos opcionales actualizados del perfil.</param>
    /// <returns>La representación DTO <see cref="UserDto"/> actualizada del usuario.</returns>
    public async Task<UserDto> UpdateProfile(Guid userId, UpdateUserRequest model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando actualizar el perfil del usuario con ID: {UserId}", userId);
        }

        var existing = await unitOfWork.Users.GetByIdAsync(userId);

        if (existing is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Usuario no encontrado para actualizar: {UserId}", userId);
            }
            throw new ResourceNotFoundException("usuario", userId);
        }

        var normalizedNickname = NormalizeOptionalProfileField(model.Nickname, nameof(model.Nickname));
        var normalizedEmail = NormalizeOptionalProfileField(model.Email, nameof(model.Email));
        var normalizedBiography = NormalizeBiography(model.Biography);

        if (normalizedEmail is not null
            && !string.Equals(existing.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase)
            && await unitOfWork.Users.ExistsByEmailAsync(normalizedEmail))
        {
            throw new AlreadyExistsException("usuario", "email", normalizedEmail);
        }

        if (normalizedNickname is not null)
        {
            existing.Nickname = normalizedNickname;
        }

        if (normalizedEmail is not null)
        {
            existing.Email = normalizedEmail;
        }

        if (model.Biography is not null)
        {
            existing.Biography = normalizedBiography;
        }

        unitOfWork.Update(existing);
        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Usuario actualizado exitosamente con ID: {UserId}", userId);
        }
        return MapToDto(existing);
    }

    /// <summary>
    /// Obtiene de forma asíncrona un listado paginado y filtrado de usuarios registrados en el sistema.
    /// </summary>
    /// <param name="limit">Cantidad máxima de usuarios a recuperar.</param>
    /// <param name="offset">Cantidad de registros a omitir para la paginación.</param>
    /// <param name="nickname">Filtro opcional para buscar por nickname (nombre de usuario).</param>
    /// <param name="email">Filtro opcional para buscar por correo electrónico.</param>
    /// <returns>Una respuesta genérica que contiene el listado de <see cref="UserDto"/> resultantes.</returns>
    public async Task<GenericResponse<List<UserDto>>> Get(int limit, int offset, string? nickname = null, string? email = null)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Obteniendo lista de usuarios | Limit: {Limit}, Offset: {Offset}, Nombre: {Nickname}, Email: {Email}",
                limit, offset, nickname, email);
        }

        var users = await unitOfWork.Users.GetAllAsync(limit, offset, nickname, email);
        var dtos = users.Select(MapToDto).ToList();
        return new GenericResponse<List<UserDto>> { Data = dtos };
    }

    /// <summary>
    /// Obtiene de forma asíncrona los detalles de un usuario individual por su identificador único.
    /// Lanza una excepción si el usuario no existe.
    /// </summary>
    /// <param name="userId">Identificador único del usuario a consultar.</param>
    /// <returns>La representación DTO <see cref="UserDto"/> del usuario.</returns>
    public async Task<UserDto> Get(Guid userId)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Buscando usuario con ID: {UserId}", userId);
        }

        var user = await unitOfWork.Users.GetByIdAsync(userId);

        if (user is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Usuario no encontrado con ID: {UserId}", userId);
            }
            throw new ResourceNotFoundException("usuario", userId);
        }

        return MapToDto(user);
    }

    /// <summary>
    /// Modifica de forma asíncrona la contraseña del usuario actual.
    /// Genera el nuevo hash criptográfico BCrypt y despacha una notificación por correo informando sobre el cambio de seguridad.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <param name="model">Modelo conteniendo la nueva contraseña solicitada.</param>
    public async Task ChangePassword(Guid userId, ChangePasswordUserRequest model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando cambiar contraseña del usuario con ID: {UserId}", userId);
        }

        var user = await unitOfWork.Users.GetByIdAsync(userId);

        if (user is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Usuario no encontrado para cambio de contraseña: {UserId}", userId);
            }
            throw new ResourceNotFoundException("usuario", userId);
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        unitOfWork.Update(user);
        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Contraseña cambiada exitosamente para usuario con ID: {UserId}", userId);
        }

        await emailService.SendPasswordChangedNotificationAsync(user.Email, user.Nickname);
    }

    /// <summary>
    /// Realiza de forma asíncrona un Soft-Delete (eliminación lógica) del usuario estableciendo el campo DeletedAt.
    /// </summary>
    /// <param name="userId">Identificador único del usuario a desactivar.</param>
    public async Task Delete(Guid userId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando soft-delete usuario con ID: {UserId}", userId);
        }

        var user = await unitOfWork.Users.GetByIdAsync(userId);

        if (user is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Usuario no encontrado para eliminar: {UserId}", userId);
            }
            throw new ResourceNotFoundException("usuario", userId);
        }

        user.DeletedAt = DateTimeHelper.UtcNow();
        unitOfWork.Update(user);
        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Usuario soft-deleted exitosamente con ID: {UserId}", userId);
        }
    }

    /// <summary>
    /// Restaura de forma asíncrona la cuenta de un usuario previamente desactivado lógicamente (Soft-Delete) limpiando la fecha de eliminación.
    /// </summary>
    /// <param name="userId">Identificador único del usuario a restaurar.</param>
    public async Task Restore(Guid userId)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intentando restaurar usuario con ID: {UserId}", userId);
        }

        var user = await unitOfWork.Users.GetByIdAsync(userId);

        if (user is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Usuario no encontrado para restaurar: {UserId}", userId);
            }
            throw new ResourceNotFoundException("usuario", userId);
        }

        user.DeletedAt = null;
        user.DeletedByAdminId = null;
        unitOfWork.Update(user);
        await unitOfWork.SaveChangesAsync();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Usuario restaurado exitosamente con ID: {UserId}", userId);
        }
    }

    /// <summary>
    /// Vincula de forma privada un rol al usuario creando el registro correspondiente en la entidad UserRole.
    /// </summary>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <param name="roleId">Identificador único del rol.</param>
    private void AssignRoleToUser(Guid userId, Guid roleId)
    {
        var userRole = new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTimeHelper.UtcNow()
        };
        unitOfWork.Create(userRole);
    }

    /// <summary>
    /// Método privado estático para mapear una entidad de base de datos <see cref="User"/> a su DTO de salida <see cref="UserDto"/> cargando su conjunto de roles.
    /// </summary>
    /// <param name="entity">Entidad de usuario a mapear.</param>
    /// <returns>El DTO <see cref="UserDto"/> resultante.</returns>
    private static UserDto MapToDto(User entity) => new()
    {
        UserId = entity.UserId,
        Nickname = entity.Nickname,
        Email = entity.Email,
        Biography = entity.Biography,
        ProfilePhotoUrl = entity.ProfilePhotoUrl,
        ProfilePhotoFileName = entity.ProfilePhotoFileName,
        IsActive = entity.IsActive,
        IsSuspended = entity.IsSuspended,
        IsShadowBanned = entity.IsShadowBanned,
        DeletedAt = entity.DeletedAt,
        CreatedAt = entity.CreatedAt,
        Roles = entity.UserRoles?.Select(ur => ur.Role?.Name ?? "").Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new List<string>()
    };

    /// <summary>
    /// Limpia y normaliza de forma estática campos opcionales del perfil del usuario, arrojando excepciones si están vacíos.
    /// </summary>
    /// <param name="value">Valor del campo.</param>
    /// <param name="fieldName">Nombre del campo para efectos de error.</param>
    /// <returns>El valor recortado y normalizado, o null si era originalmente nulo.</returns>
    private static string? NormalizeOptionalProfileField(string? value, string fieldName)
    {
        if (value is null)
        {
            return null;
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Length == 0)
        {
            throw new ValidationException($"El campo {fieldName} no puede estar vacío");
        }

        return normalizedValue;
    }

    /// <summary>
    /// Limpia y normaliza el campo de biografía opcional del usuario.
    /// </summary>
    /// <param name="biography">Biografía del usuario en crudo.</param>
    /// <returns>El string normalizado o null si quedó vacío.</returns>
    private static string? NormalizeBiography(string? biography)
    {
        if (biography is null)
        {
            return null;
        }

        var normalizedBiography = biography.Trim();
        return normalizedBiography.Length == 0 ? null : normalizedBiography;
    }
}
