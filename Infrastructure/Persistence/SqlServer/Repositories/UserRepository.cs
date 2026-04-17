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

    public List<User> GetAll(int limit, int offset, string? fullName = null, string? email = null)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(fullName))
            query = query.Where(u => u.FullName.Contains(fullName));

        if (!string.IsNullOrWhiteSpace(email))
            query = query.Where(u => u.Email.Contains(email));

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return query.Skip(normalizedOffset).Take(normalizedLimit).ToList();
    }

    public User? GetByEmail(string email) 
        => _context.Users.FirstOrDefault(u => u.Email == email);

    public bool ExistsByEmail(string email) 
        => _context.Users.Any(u => u.Email == email);

    public string? GetPasswordHash(Guid userId)
    {
        var user = _context.Users.Find(userId);
        return user?.PasswordHash;
    }

    public bool VerifyPassword(string passwordHash, string password)
        => BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
