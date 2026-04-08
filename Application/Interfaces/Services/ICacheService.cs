namespace Application.Interfaces.Services;

/// <summary>
/// Servicio de caché en memoria para almacenar datos temporalmente.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Guarda un valor en el caché con tiempo de expiración absoluto.
    /// </summary>
    /// <typeparam name="T">Tipo del valor</typeparam>
    /// <param name="key">Clave única</param>
    /// <param name="value">Valor a guardar</param>
    /// <param name="expiration">Tiempo de expiración</param>
    /// <returns>El valor guardado</returns>
    T Set<T>(string key, T value, TimeSpan expiration);

    /// <summary>
    /// Guarda un valor en el caché con tiempo de expiración absoluto (sobrecarga con DateTimeOffset).
    /// </summary>
    T Set<T>(string key, T value, DateTimeOffset absoluteExpiration);

    /// <summary>
    /// Obtiene o crea un valor en el caché. Si existe, lo devuelve; si no, ejecuta la factory y lo guarda.
    /// </summary>
    /// <typeparam name="T">Tipo del valor</typeparam>
    /// <param name="key">Clave única</param>
    /// <param name="factory">Función que genera el valor si no existe</param>
    /// <param name="expiration">Tiempo de expiración</param>
    /// <returns>El valor del caché o el resultado de la factory</returns>
    T GetOrCreate<T>(string key, Func<T> factory, TimeSpan expiration);

    /// <summary>
    /// Obtiene un valor del caché.
    /// </summary>
    /// <typeparam name="T">Tipo del valor esperado</typeparam>
    /// <param name="key">Clave única</param>
    /// <returns>El valor o null si no existe</returns>
    T? Get<T>(string key);

    /// <summary>
    /// Intenta obtener un valor del caché.
    /// </summary>
    /// <typeparam name="T">Tipo del valor esperado</typeparam>
    /// <param name="key">Clave única</param>
    /// <param name="value">Out parameter con el valor</param>
    /// <returns>True si existe, False si no</returns>
    bool TryGet<T>(string key, out T? value);

    /// <summary>
    /// Verifica si una clave existe en el caché.
    /// </summary>
    bool Exists(string key);

    /// <summary>
    /// Elimina una clave del caché.
    /// </summary>
    /// <param name="key">Clave a eliminar</param>
    /// <returns>True si existía y se eliminó, False si no existía</returns>
    bool Remove(string key);

    /// <summary>
    /// Elimina múltiples claves que coincidan con un patrón.
    /// </summary>
    /// <param name="pattern">Patrón de búsqueda (ej: "user:")</param>
    /// <returns>Cantidad de claves eliminadas</returns>
    int RemoveByPattern(string pattern);

    /// <summary>
    /// Limpia todo el caché.
    /// </summary>
    void Clear();
}
