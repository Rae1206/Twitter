namespace Application.Models.Cache;

/// <summary>
/// Información del token guardada en caché para permitir rotación segura.
/// </summary>
public class CachedTokenInfo
{
    /// <summary>
    /// El token JWT actual (cambia en cada rotación).
    /// </summary>
    public string CurrentToken { get; set; } = null!;

    /// <summary>
    /// Cuándo se hizo login originalmente (límite de 24h).
    /// </summary>
    public DateTime LoginTime { get; set; }

    /// <summary>
    /// Cuándo fue la última vez que se rotó el token.
    /// </summary>
    public DateTime LastRotation { get; set; }

    /// <summary>
    /// Datos del usuario para regenerar el token.
    /// </summary>
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = null!;

    /// <summary>
    /// Verifica si ya pasaron 24 horas desde el login.
    /// </summary>
    public bool IsExpired(TimeSpan maxSessionDuration)
    {
        return DateTime.UtcNow - LoginTime > maxSessionDuration;
    }

    /// <summary>
    /// Calcula cuánto tiempo queda de sesión válida.
    /// </summary>
    public TimeSpan GetRemainingTime(TimeSpan maxSessionDuration)
    {
        var remaining = maxSessionDuration - (DateTime.UtcNow - LoginTime);
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }
}
