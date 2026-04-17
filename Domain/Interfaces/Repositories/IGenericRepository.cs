using System.Linq.Expressions;

namespace Twitter.Domain.Interfaces.Repositories;

/// <summary>
/// Interfaz genérica para repositorios CRUD.
/// </summary>
/// <typeparam name="T">Tipo de entidad</typeparam>
/// <typeparam name="TKey">Tipo de clave primaria</typeparam>
public interface IGenericRepository<T, TKey> where T : class
{
    T Create(T entity);
    T? GetById(TKey id);
    List<T> GetAll(int limit = 0, int offset = 0, Expression<Func<T, bool>>? filter = null);
    Task<bool> IfExists(Expression<Func<T, bool>> expression);
    T? Update(TKey id, T entity);
    bool Delete(TKey id);
    bool Exists(TKey id);
    T? GetByField(Expression<Func<T, bool>> expression);
}
