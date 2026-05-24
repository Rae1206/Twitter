namespace Shared.Constants;

/// <summary>
/// Constantes relacionadas con publicaciones (posts).
/// </summary>
public static class PostConstants
{
    /// <summary>
    /// Duración mínima permitida para un post efímero (en minutos).
    /// </summary>
    public const int MinEphemeralMinutes = 1;

    /// <summary>
    /// Duración máxima permitida para un post efímero (en minutos). Equivale a 72 horas (3 días).
    /// </summary>
    public const int MaxEphemeralMinutes = 4320;

    /// <summary>
    /// Antigüedad mínima de un post efímero ya vencido antes de que el background service lo soft-delete.
    /// Damos un margen de 1 minuto sobre ExpiresAt para evitar carreras con clientes que estén
    /// renderizando el countdown final.
    /// </summary>
    public const int EphemeralCleanupGraceMinutes = 1;
}
