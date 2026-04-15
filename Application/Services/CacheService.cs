using System.Collections.Concurrent;
using Application.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Application.Services;

/// <summary>
/// Implementación de caché en memoria con funcionalidades extendidas.
/// Usa IMemoryCache de ASP.NET Core con tracking de claves para operaciones avanzadas.
/// </summary>
public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheService> _logger;
    
    // Tracking de claves para soportar operaciones como RemoveByPattern
    // IMemoryCache nativo no expone las claves, así que las trackeamos nosotros
    private readonly ConcurrentDictionary<string, bool> _keyTracker = new();

    public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public T Set<T>(string key, T value, TimeSpan expiration)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("La clave no puede estar vacía", nameof(key));

        try
        {
            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expiration)
                .RegisterPostEvictionCallback(OnPostEviction);

            _memoryCache.Set(key, value, options);
            _keyTracker.TryAdd(key, true);
            
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Valor guardado en caché con clave: {Key}, expiración: {Expiration}s", 
                    key, expiration.TotalSeconds);
            }
            
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar en caché con clave: {Key}", key);
            throw new InvalidOperationException($"No se pudo guardar el valor en caché con clave '{key}'", ex);
        }
    }

    
    public T Set<T>(string key, T value, DateTimeOffset absoluteExpiration)
    {
        var expiration = absoluteExpiration - DateTimeOffset.UtcNow;
        return Set(key, value, expiration);
    }

    public T GetOrCreate<T>(string key, Func<T> factory, TimeSpan expiration)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("La clave no puede estar vacía", nameof(key));

        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        try
        {
            // Intentar obtener del caché primero
            if (_memoryCache.TryGetValue(key, out T? cachedValue) && cachedValue is not null)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Valor recuperado del caché: {Key}", key);
                }
                return cachedValue;
            }

            // Si no existe, ejecutar la factory
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Valor no encontrado en caché, ejecutando factory: {Key}", key);
            }
            var value = factory();
            
            if (value is not null)
            {
                Set(key, value, expiration);
            }
            
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetOrCreate para clave: {Key}", key);
            throw;
        }
    }

    public T? Get<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return default;

        try
        {
            if (_memoryCache.TryGetValue(key, out T? value))
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Cache HIT: {Key}", key);
                }
                return value;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Cache MISS: {Key}", key);
            }
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener del caché: {Key}", key);
            return default;
        }
    }

    /// <inheritdoc />
    public bool TryGet<T>(string key, out T? value)
    {
        value = default;
        
        if (string.IsNullOrWhiteSpace(key))
            return false;

        try
        {
            if (_memoryCache.TryGetValue(key, out T? cachedValue))
            {
                value = cachedValue;
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en TryGet para clave: {Key}", key);
            return false;
        }
    }

    public bool Exists(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        try
        {
            return _memoryCache.TryGetValue(key, out _);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar existencia: {Key}", key);
            return false;
        }
    }

    public bool Remove(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        try
        {
            if (!Exists(key))
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Intento de eliminar clave inexistente: {Key}", key);
                }
                return false;
            }

            _memoryCache.Remove(key);
            _keyTracker.TryRemove(key, out _);
            
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Clave eliminada del caché: {Key}", key);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar del caché: {Key}", key);
            return false;
        }
    }

    public int RemoveByPattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return 0;

        try
        {
            var keysToRemove = _keyTracker
                .Where(kvp => kvp.Key.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                .Select(kvp => kvp.Key)
                .ToList();

            int removedCount = 0;
            foreach (var key in keysToRemove)
            {
                if (Remove(key))
                    removedCount++;
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Eliminadas {Count} claves con patrón '{Pattern}'", 
                    removedCount, pattern);
            }
            
            return removedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar por patrón: {Pattern}", pattern);
            return 0;
        }
    }

    public void Clear()
    {
        try
        {
            // Como no podemos limpiar IMemoryCache directamente (no tiene Clear),
            // removemos todas las claves trackeadas
            var keys = _keyTracker.Keys.ToList();
            int removedCount = 0;
            
            foreach (var key in keys)
            {
                _memoryCache.Remove(key);
                _keyTracker.TryRemove(key, out _);
                removedCount++;
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Caché limpiado. Eliminadas {Count} entradas", removedCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al limpiar el caché");
        }
    }

    /// <summary>
    /// Callback ejecutado cuando una entrada es removida del caché (expiración, evicción, etc.)
    /// </summary>
    private void OnPostEviction(object key, object? value, EvictionReason reason, object? state)
    {
        var keyString = key?.ToString() ?? "unknown";
        _keyTracker.TryRemove(keyString, out _);
        
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Entrada removida del caché: {Key}, Razón: {Reason}", keyString, reason);
        }
    }
}
