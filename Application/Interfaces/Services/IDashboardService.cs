using Application.Models.Responses;

namespace Application.Interfaces.Services;

public interface IDashboardService
{
    Task<GenericResponse<Dictionary<string, decimal>>> GetStatsAsync();
    Task RecalculateStatsAsync();
}
