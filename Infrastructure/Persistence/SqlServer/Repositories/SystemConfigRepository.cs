using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de base de datos específico para el almacenamiento, actualización y obtención de variables dinámicas y configuraciones clave del sistema.
/// </summary>
public class SystemConfigRepository : GenericRepository<SystemConfig, Guid>, ISystemConfigRepository
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="SystemConfigRepository"/> con el contexto de base de datos.
    /// </summary>
    /// <param name="context">El contexto de base de datos <see cref="TwitterDbContext"/>.</param>
    public SystemConfigRepository(TwitterDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene de forma asíncrona una clave de configuración específica mediante su identificativo textual único.
    /// </summary>
    /// <param name="key">Nombre de la clave de configuración.</param>
    /// <returns>La entidad de configuración <see cref="SystemConfig"/> o null si no se encuentra registrada.</returns>
    public async Task<SystemConfig?> GetByKeyAsync(string key)
    {
        return await _context.SystemConfigs.FirstOrDefaultAsync(c => c.Key == key);
    }

    /// <summary>
    /// Obtiene de forma asíncrona todas las variables y configuraciones editables del sistema.
    /// </summary>
    /// <returns>Una lista de todas las configuraciones <see cref="SystemConfig"/> registradas.</returns>
    public async Task<List<SystemConfig>> GetAllEditableAsync()
    {
        return await _context.SystemConfigs.ToListAsync();
    }
}
