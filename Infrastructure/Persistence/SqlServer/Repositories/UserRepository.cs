using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación del repositorio de usuarios.
/// </summary>
public class UserRepository : GenericRepository<User, Guid>, IUserRepository
{
    public UserRepository(TwitterDbContext context) : base(context)
    {
    }

    public override User Create(User user)
    {
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        _dbSet.Add(user);
        return user;
    }

    public override User? GetById(Guid id) =>
        GetByField(u => u.UserId == id);

    public List<User> GetAll(int limit, int offset, string? fullName = null, string? email = null)
    {
        Expression<Func<User, bool>>? filter = null;

        if (!string.IsNullOrWhiteSpace(fullName) || !string.IsNullOrWhiteSpace(email))
        {
            Expression<Func<User, bool>> expr = u => true;
            var param = expr.Parameters[0];

            if (!string.IsNullOrWhiteSpace(fullName))
                expr = u => u.FullName.Contains(fullName);
            
            if (!string.IsNullOrWhiteSpace(email))
                expr = u => u.Email.Contains(email);

            filter = u => (fullName == null || u.FullName.Contains(fullName)) 
                       && (email == null || u.Email.Contains(email));
        }

        return base.GetAll(limit, offset, filter);
    }

    public override User? Update(Guid id, User user)
    {
        var entity = _dbSet.Find(id);
        if (entity is null) return null;

        entity.FullName = user.FullName;
        entity.Email = user.Email;
        entity.IsActive = user.IsActive;

        return entity;
    }

    public User? GetByEmail(string email) =>
        GetByField(u => u.Email == email);

    public bool ExistsByEmail(string email) => 
        _dbSet.Any(u => u.Email == email);

    public bool ChangePassword(Guid userId, string newPassword)
    {
        var entity = _dbSet.Find(userId);
        if (entity is null) return false;

        entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        return true;
    }

    public bool VerifyPassword(Guid userId, string password)
    {
        var entity = _dbSet.Find(userId);
        if (entity is null) return false;

        return BCrypt.Net.BCrypt.Verify(password, entity.PasswordHash);
    }
}