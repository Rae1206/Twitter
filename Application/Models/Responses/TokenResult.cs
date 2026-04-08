namespace Application.Models.Responses;

/// <summary>
/// Resultado de la operación de rotación de token.
/// </summary>
public class TokenResult
{
    /// <summary>
    /// El token JWT (puede ser el mismo o uno nuevo si se rotó).
    /// </summary>
    public string Token { get; set; } = null!;

    /// <summary>
    /// Indica si el token fue rotado (generado uno nuevo) o se usó el existente.
    /// </summary>
    public bool WasRotated { get; set; }

    /// <summary>
    /// Tiempo restante de la sesión de 24 horas.
    /// </summary>
    public TimeSpan RemainingSessionTime { get; set; }
}
