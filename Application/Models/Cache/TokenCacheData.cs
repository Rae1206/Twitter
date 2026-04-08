namespace Application.Models.Cache;

/// <summary>
/// Datos guardados en caché para permitir regeneración automática del token.
/// </summary>
public class TokenCacheData
{
    public string Token { get; set; } = null!;
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
