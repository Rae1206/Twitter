using Application.Models.Responses;
using Twitter.Domain.Database.SqlServer.Entities;

namespace Application.Interfaces.Services;

public interface IConfigService
{
    Task<SystemConfig?> GetConfigAsync(string key);
    Task<SystemConfig> UpdateConfigAsync(string key, string value, Guid adminId);
    Task<GenericResponse<List<SystemConfig>>> GetAllConfigsAsync();
}
