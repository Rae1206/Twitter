using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User, Guid>
{
    Task<List<User>> GetAllAsync(int limit, int offset, string? nickname = null, string? email = null);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email);
    Task<string?> GetPasswordHashAsync(Guid userId);
}
