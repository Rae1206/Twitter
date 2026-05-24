using Microsoft.EntityFrameworkCore;
using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositorio de usuarios.
/// Hereda GenericRepository para lectura.
/// Usa UnitOfWork para escritura.
/// </summary>
public class UserRepository : GenericRepository<User, Guid>, IUserRepository
{
    public UserRepository(TwitterDbContext context) : base(context)
    {
    }

    public async Task<List<User>> GetAllAsync(int limit, int offset, string? fullName = null, string? email = null)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(fullName))
            query = query.Where(u => u.FullName.Contains(fullName));

        if (!string.IsNullOrWhiteSpace(email))
            query = query.Where(u => u.Email.Contains(email));

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return await query.Skip(normalizedOffset).Take(normalizedLimit).ToListAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<bool> ExistsByEmailAsync(string email)
        => await _context.Users.AnyAsync(u => u.Email == email);

    public async Task<string?> GetPasswordHashAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user?.PasswordHash;
    }
}
