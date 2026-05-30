using Application.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Services;

/// <summary>
/// Implementación del servicio de caché en memoria usando IMemoryCache.
/// </summary>
public class CacheService(IMemoryCache memoryCache) : ICacheService
{
    /// <summary>
    /// Crea o actualiza un valor en la caché con un tiempo de expiración determinado.
    /// </summary>
    /// <typeparam name="T">Tipo del elemento a almacenar en caché.</typeparam>
    /// <param name="key">Clave única para identificar el elemento.</param>
    /// <param name="expiration">Tiempo de duración de la caché antes de expirar.</param>
    /// <param name="value">Valor o datos que se desean guardar.</param>
    /// <returns>El valor guardado en caché.</returns>
    public T Create<T>(string key, TimeSpan expiration, T value)
    {
        var create = memoryCache.GetOrCreate(key, (factory) =>
        {
            factory.SlidingExpiration = expiration;
            return value;
        });
        return create is null ? throw new Exception("No se pudo establecer la caché") : create;
    }

    /// <summary>
    /// Elimina un elemento de la caché por su clave.
    /// </summary>
    /// <param name="key">Clave única del elemento a eliminar.</param>
    /// <returns>Verdadero si el elemento fue removido de la caché.</returns>
    public bool Delete(string key)
    {
        memoryCache.Remove(key);
        return true;
    }

    /// <summary>
    /// Recupera un elemento almacenado en la caché utilizando su clave única.
    /// </summary>
    /// <typeparam name="T">Tipo esperado del elemento en caché.</typeparam>
    /// <param name="key">Clave única del elemento a recuperar.</param>
    /// <returns>El valor almacenado o null si no se encuentra o ha expirado.</returns>
    public T? Get<T>(string key)
    {
        return memoryCache.Get<T>(key);
    }
}