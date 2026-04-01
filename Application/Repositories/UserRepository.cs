using Application.Interfaces.Repositories;
using Application.Models.DTOs;

namespace Application.Repositories;

public class UserRepository : IUserRepository
{
    // TODO: Implement with Entity Framework Core
    // private readonly AppDbContext _context;

    public UserDto Create(UserDto user)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public bool Delete(Guid userId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public bool Exists(Guid userId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public List<UserDto> GetAll(int limit, int offset)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public UserDto? GetById(Guid userId)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public UserDto? Update(Guid userId, UserDto user)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }

    public bool ChangePassword(Guid userId, string newPassword)
    {
        throw new NotImplementedException("Implementar con EF Core");
    }
}