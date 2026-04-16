using Twitter.Domain.Database.SqlServer.Context;
using Twitter.Domain.Database.SqlServer.Entities;
using Twitter.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

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

    public override User? GetById(Guid id)
    {
        return _dbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefault(u => u.UserId == id);
    }

    public new List<User> GetAll(int limit, int offset, string? fullName = null, string? email = null)
    {
        var query = _dbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(fullName))
            query = query.Where(u => u.FullName.Contains(fullName));

        if (!string.IsNullOrWhiteSpace(email))
            query = query.Where(u => u.Email.Contains(email));

        var normalizedOffset = Math.Max(offset, 0);
        var normalizedLimit = limit <= 0 ? int.MaxValue : limit;

        return query
            .Skip(normalizedOffset)
            .Take(normalizedLimit)
            .ToList();
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
        _dbSet.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefault(u => u.Email == email);

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