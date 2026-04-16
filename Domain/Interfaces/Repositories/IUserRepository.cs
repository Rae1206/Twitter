using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz del repositorio de usuarios.
/// </summary>
public interface IUserRepository : IGenericRepository<User, Guid>
{
    // GetAll con filtros adicionales
    List<User> GetAll(int limit, int offset, string? fullName = null, string? email = null);
    
    // Métodos específicos de User
    User? GetByEmail(string email);
    bool ExistsByEmail(string email);
    bool ChangePassword(Guid userId, string newPassword);
    bool VerifyPassword(Guid userId, string password);
}