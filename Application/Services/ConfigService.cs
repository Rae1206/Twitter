using Application.Interfaces.Services;
using Application.Models.Responses;
using Microsoft.Extensions.Logging;
using Twitter.Domain.Exceptions;
using Shared.Helpers;
using Twitter.Domain.Interfaces;
using Twitter.Domain.Database.SqlServer.Entities;

namespace Application.Services;

/// <summary>
/// Servicio de lógica de negocio para gestionar las configuraciones dinámicas del sistema.
/// </summary>
public class ConfigService(
    IUnitOfWork unitOfWork,
    IAuditService auditService,
    ILogger<ConfigService> logger) : IConfigService
{
    /// <summary>
    /// Obtiene de forma asíncrona una configuración del sistema específica por su clave única.
    /// </summary>
    /// <param name="key">Clave única de la configuración.</param>
    /// <returns>La entidad de configuración del sistema encontrada o null.</returns>
    public async Task<SystemConfig?> GetConfigAsync(string key)
    {
        return await unitOfWork.SystemConfigs.GetByKeyAsync(key);
    }

    /// <summary>
    /// Actualiza de forma asíncrona el valor de una configuración del sistema y registra la acción en la bitácora de auditoría.
    /// </summary>
    /// <param name="key">Clave única de la configuración que se va a actualizar.</param>
    /// <param name="value">Nuevo valor asignado a la configuración.</param>
    /// <param name="adminId">Identificador único del administrador que realiza la actualización.</param>
    /// <returns>La entidad de configuración del sistema actualizada.</returns>
    public async Task<SystemConfig> UpdateConfigAsync(string key, string value, Guid adminId)
    {
        var config = await unitOfWork.SystemConfigs.GetByKeyAsync(key);
        if (config is null)
        {
            throw new ResourceNotFoundException("configuración", key);
        }

        var oldValue = config.Value;
        config.Value = value;
        config.UpdatedAt = DateTimeHelper.UtcNow();
        config.UpdatedByUserId = adminId;
        unitOfWork.Update(config);
        await unitOfWork.SaveChangesAsync();

        await auditService.LogChangeAsync(adminId, "UPDATE_CONFIG", "SystemConfig", key, oldValue, value);

        return config;
     }

    /// <summary>
    /// Obtiene de forma asíncrona el listado completo de configuraciones clave-valor registradas en el sistema.
    /// </summary>
    /// <returns>Un sobre genérico de respuesta que envuelve la lista completa de configuraciones encontradas.</returns>
    public async Task<GenericResponse<List<SystemConfig>>> GetAllConfigsAsync()
    {
        var configs = await unitOfWork.SystemConfigs.GetAllAsync();
        return new GenericResponse<List<SystemConfig>> { Data = configs };
    }
}
