using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de autenticación - solo lectura.
/// </summary>
public class AuthRepository : GenericRepository<User, Guid>, IAuthRepository
{
    public AuthRepository(TwitterDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email) => await _context.Users
        .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
        .FirstOrDefaultAsync(u => u.Email == email);
}
