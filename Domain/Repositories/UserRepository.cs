using Domain.Context;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Domain.Repositories;

public class UserRepository(TwitterDbContext context) : IUserRepository
{
    public User Create(User user)
    {
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        
        context.Users.Add(user);
        context.SaveChanges();
        
        return user;
    }

    public User? GetById(Guid userId) => 
        context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefault(u => u.UserId == userId);

    public User? GetByEmail(string email) =>
        context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefault(u => u.Email == email);

    public List<User> GetAll(int limit, int offset, string? fullName = null, string? email = null)
    {
        var query = context.Users
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

    public User? Update(Guid userId, User user)
    {
        var entity = context.Users.Find(userId);
        if (entity is null) return null;

        entity.FullName = user.FullName;
        entity.Email = user.Email;
        entity.IsActive = user.IsActive;

        context.SaveChanges();
        return entity;
    }

    public bool Delete(Guid userId)
    {
        var entity = context.Users.Find(userId);
        if (entity is null) return false;

        context.Users.Remove(entity);
        context.SaveChanges();
        return true;
    }

    public bool ChangePassword(Guid userId, string newPassword)
    {
        var entity = context.Users.Find(userId);
        if (entity is null) return false;

        entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        context.SaveChanges();
        return true;
    }

    public bool Exists(Guid userId) => context.Users.Any(u => u.UserId == userId);

    public bool ExistsByEmail(string email) => context.Users.Any(u => u.Email == email);

    public bool VerifyPassword(Guid userId, string password)
    {
        var entity = context.Users.Find(userId);
        if (entity is null) return false;

        return BCrypt.Net.BCrypt.Verify(password, entity.PasswordHash);
    }
}
