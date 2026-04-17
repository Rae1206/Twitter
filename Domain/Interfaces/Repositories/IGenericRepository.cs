using System.Linq.Expressions;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz genérica para operaciones de CONSULTA.
/// Las operaciones de escritura se manejan en UnitOfWork.
/// </summary>
public interface IGenericRepository<T, TKey> where T : class
{
    T? GetById(TKey id);
    List<T> GetAll(int limit = 0, int offset = 0, Expression<Func<T, bool>>? filter = null);
    Task<bool> IfExists(Expression<Func<T, bool>> expression);
    bool Exists(TKey id);
    T? GetByField(Expression<Func<T, bool>> expression);
}