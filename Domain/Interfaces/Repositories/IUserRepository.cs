using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz del repositorio de usuarios (solo lectura).
/// </summary>
public interface IUserRepository : IGenericRepository<User, Guid>
{
    List<User> GetAll(int limit, int offset, string? fullName = null, string? email = null);
    User? GetByEmail(string email);
    bool ExistsByEmail(string email);
    string? GetPasswordHash(Guid userId);
    bool VerifyPassword(string passwordHash, string password);
}
