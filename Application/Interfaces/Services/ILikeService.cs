using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twitter.Domain.Database.SqlServer.Entities;

namespace Application.Interfaces.Services;

public interface ILikeService
{
    Task ToggleLike(Guid postId, Guid userId);
    Task<List<User>> GetLikers(Guid postId, int limit = 0, int offset = 0);
}
