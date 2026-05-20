using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

public interface IUserSuspensionRepository : IGenericRepository<UserSuspension, Guid>
{
    Task<UserSuspension?> GetActiveSuspensionAsync(Guid userId);
}
