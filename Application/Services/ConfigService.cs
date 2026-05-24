using Application.Interfaces.Services;
using Application.Models.Responses;
using Microsoft.Extensions.Logging;
using Twitter.Domain.Exceptions;
using Shared.Helpers;
using Twitter.Domain.Interfaces;
using Twitter.Domain.Database.SqlServer.Entities;

namespace Application.Services;

public class ConfigService(
    IUnitOfWork unitOfWork,
    IAuditService auditService,
    ILogger<ConfigService> logger) : IConfigService
{
    public async Task<SystemConfig?> GetConfigAsync(string key)
    {
        return await unitOfWork.SystemConfigs.GetByKeyAsync(key);
    }

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

    public async Task<GenericResponse<List<SystemConfig>>> GetAllConfigsAsync()
    {
        var configs = await unitOfWork.SystemConfigs.GetAllAsync();
        return new GenericResponse<List<SystemConfig>> { Data = configs };
    }
}
