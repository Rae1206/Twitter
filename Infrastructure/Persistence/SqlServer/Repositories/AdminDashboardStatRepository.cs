using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de base de datos específico para el almacenamiento y recalculado de métricas y estadísticas del panel de administración.
/// </summary>
public class AdminDashboardStatRepository : GenericRepository<AdminDashboardStat, Guid>, IAdminDashboardStatRepository
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="AdminDashboardStatRepository"/> con el contexto de base de datos.
    /// </summary>
    /// <param name="context">El contexto de base de datos <see cref="TwitterDbContext"/>.</param>
    public AdminDashboardStatRepository(TwitterDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene de forma asíncrona todas las estadísticas guardadas del panel de control de administración.
    /// </summary>
    /// <returns>Una lista de todas las estadísticas de administración <see cref="AdminDashboardStat"/>.</returns>
    public async Task<List<AdminDashboardStat>> GetAllAsync()
    {
        return await _context.AdminDashboardStats.ToListAsync();
    }

    /// <summary>
    /// Realiza de forma asíncrona un Upsert (actualiza el valor si existe, o inserta uno nuevo si no existe) para una estadística clave específica.
    /// </summary>
    /// <param name="stat">La entidad conteniendo el valor y clave a persistir.</param>
    public async Task UpsertAsync(AdminDashboardStat stat)
    {
        var existing = await _context.AdminDashboardStats.FirstOrDefaultAsync(s => s.StatKey == stat.StatKey);
        if (existing is not null)
        {
            existing.StatValue = stat.StatValue;
            existing.LastCalculated = stat.LastCalculated;
            _context.Update(existing);
        }
        else
        {
            _context.Add(stat);
        }
    }
}
