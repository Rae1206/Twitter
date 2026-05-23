using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Twitter.Domain.Database.SqlServer.Entities;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz del repositorio de likes.
/// Hereda de IGenericRepository para operaciones CRUD genéricas.
/// </summary>
public interface ILikeRepository : IGenericRepository<Like, Guid>
{
    Task<Like?> GetLike(Guid userId, Guid postId);
    Task<List<User>> GetLikers(Guid postId, int limit = 0, int offset = 0);
}
