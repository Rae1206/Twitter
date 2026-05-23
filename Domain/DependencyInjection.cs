using Twitter.Domain.Database.SqlServer.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Twitter.Domain;

/// <summary>
/// Extensión para registrar la configuración de dominio.
/// </summary>
public static class DependencyInjection
{
    private const string DefaultConnection = "DefaultConnection";

    /// <summary>
    /// Registra el DbContext y configuraciones de base de datos.
    /// </summary>
    public static IServiceCollection AddDomainServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(DefaultConnection);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("No SQL Server connection string has been configured.");
        }

        services.AddDbContext<TwitterDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }
}
