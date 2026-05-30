using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Twitter.Domain.Database.SqlServer.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de base de datos específico para la consulta y obtención de las plantillas HTML de los correos electrónicos del sistema.
/// </summary>
public class EmailTemplateRepository : GenericRepository<EmailTemplate, int>, IEmailTemplateRepository
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="EmailTemplateRepository"/> con el contexto de base de datos.
    /// </summary>
    /// <param name="context">El contexto de base de datos <see cref="TwitterDbContext"/>.</param>
    public EmailTemplateRepository(TwitterDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene de forma asíncrona una plantilla de correo electrónico buscando por su nombre identificativo único en base de datos.
    /// </summary>
    /// <param name="name">Nombre único de la plantilla.</param>
    /// <returns>La entidad de plantilla de correo <see cref="EmailTemplate"/> o null si no se encuentra.</returns>
    public async Task<EmailTemplate?> GetByNameAsync(string name)
    {
        return await _context.Set<EmailTemplate>()
            .FirstOrDefaultAsync(t => t.Name == name);
    }
}
