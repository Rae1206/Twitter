using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

public interface IAuthRepository : IGenericRepository<User, Guid>
{
    Task<User?> GetByEmailAsync(string email);
}
