using Twitter.Domain.Interfaces.Repositories;

namespace Twitter.Domain.Database.SqlServer;

/// <summary>
/// Interfaz Unit of Work.
/// Define operaciones de ESCRITURA y acceso a repositorios.
/// </summary>
public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IPostRepository Posts { get; }
    IAuthRepository Auth { get; }
    IRoleRepository Roles { get; }

    // ============================================
    // OPERACIONES DE ESCRITURA
    // ============================================
    void Create<T>(T entity) where T : class;
    void Update<T>(T entity) where T : class;
    void Delete<T>(T entity) where T : class;

    // ============================================
    // TRANSACCIONES
    // ============================================
    Task SaveChangesAsync();
}
