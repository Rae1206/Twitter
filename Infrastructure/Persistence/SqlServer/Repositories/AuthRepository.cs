using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación del repositorio de autenticación.
/// </summary>
public class AuthRepository : GenericRepository<User, Guid>, IAuthRepository
{
    public AuthRepository(TwitterDbContext context) : base(context)
    {
    }

    public override User? GetByField(Expression<Func<User, bool>> expression) =>
        _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefault(expression);

    public User? GetByEmail(string email) => GetByField(u => u.Email == email);

    public new User? GetById(Guid id) => GetByField(u => u.UserId == id);

    public bool VerifyPassword(Guid userId, string password)
    {
        var entity = _dbSet.Find(userId);
        if (entity is null) return false;

        return BCrypt.Net.BCrypt.Verify(password, entity.PasswordHash);
    }
}