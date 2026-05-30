using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz específica para el repositorio de plantillas de correo electrónico.
/// </summary>
public interface IEmailTemplateRepository
{
    /// <summary>
    /// Obtiene de forma asíncrona una plantilla de correo electrónico buscando por su nombre identificativo único.
    /// </summary>
    /// <param name="name">Nombre único de la plantilla.</param>
    /// <returns>La entidad de la plantilla de correo <see cref="EmailTemplate"/> o null si no existe.</returns>
    Task<EmailTemplate?> GetByNameAsync(string name);
}
