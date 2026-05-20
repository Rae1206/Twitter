using Application.Models.Responses;

namespace Application.Interfaces.Services;

public interface IDashboardService
{
    GenericResponse<Dictionary<string, decimal>> GetStatsAsync();
    Task RecalculateStatsAsync();
}
