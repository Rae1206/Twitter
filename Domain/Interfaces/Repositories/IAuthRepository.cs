using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz del repositorio de autenticación.
/// </summary>
public interface IAuthRepository : IGenericRepository<User, Guid>
{
    User? GetByEmail(string email);
    bool VerifyPassword(Guid userId, string password);
}