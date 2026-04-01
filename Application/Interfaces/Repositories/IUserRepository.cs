using Application.Models.DTOs;

namespace Application.Interfaces.Repositories;

public interface IUserRepository
{
    UserDto Create(UserDto user);
    UserDto? GetById(Guid userId);
    List<UserDto> GetAll(int limit, int offset);
    UserDto? Update(Guid userId, UserDto user);
    bool Delete(Guid userId);
    bool ChangePassword(Guid userId, string newPassword);
    bool Exists(Guid userId);
}