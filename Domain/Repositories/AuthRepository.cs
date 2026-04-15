using Domain.Context;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Domain.Repositories;

public class AuthRepository(TwitterDbContext context) : IAuthRepository
{
    public User? GetByEmail(string email) =>
        context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefault(u => u.Email == email);

    public bool VerifyPassword(Guid userId, string password)
    {
        var entity = context.Users.Find(userId);
        if (entity is null) return false;

        return BCrypt.Net.BCrypt.Verify(password, entity.PasswordHash);
    }
}
