using Application.Interfaces.Services;
using Application.Models.Cache;
using Application.Models.DTOs;
using Application.Models.Requests.User;
using Application.Models.Responses;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Configuration;
using Shared.Constants;
using Shared.Exceptions;
using Shared.Helpers;

namespace Application.Services;

/// <summary>
/// Servicio de autenticación con rotación de tokens cada 1-5 minutos.
/// La sesión total dura 24 horas desde el login.
/// </summary>
public class AuthService(
    IAuthRepository authRepository,
    IUserRepository userRepository,
    IOptions<TokenConfiguration> tokenOptions,
    ICacheService cacheService,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly TokenConfiguration _tokenConfig = tokenOptions.Value;

    public LoginResponse Login(LoginUserRequest model)
    {
        logger.LogInformation("Intento de inicio de sesión con email: {Email}", model.Email);

        var user = authRepository.GetByEmail(model.Email);

        if (user is null)
        {
            logger.LogWarning("Credenciales inválidas para email: {Email}", model.Email);
            throw new UnauthorizedAccessException(ErrorConstants.INVALID_CREDENTIALS);
        }

        if (!user.IsActive)
        {
            logger.LogWarning("Intento de inicio de sesión con cuenta deshabilitada: {Email}", model.Email);
            throw new ForbiddenException(ErrorConstants.ACCOUNT_DISABLED);
        }

        if (!authRepository.VerifyPassword(user.UserId, model.Password))
        {
            logger.LogWarning("Contraseña incorrecta para email: {Email}", model.Email);
            throw new UnauthorizedAccessException(ErrorConstants.INVALID_CREDENTIALS);
        }

        // Crear info de token con tiempo de login
        var tokenInfo = CreateTokenInfo(user.UserId, user.FullName, user.Role);

        // Calcular expiración del JWT (24 horas desde ahora)
        var jwtExpiration = _tokenConfig.GetExpirationDate();

        logger.LogInformation("Inicio de sesión exitoso para usuario: {Email} | ID: {UserId} | Sesión: 24h",
            model.Email, user.UserId);

        return new LoginResponse
        {
            Token = tokenInfo.CurrentToken,
            ExpiresAt = jwtExpiration,
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role
        };
    }

    /// <summary>
    /// Obtiene un token del caché o lo rota automáticamente si expiró.
    /// Si pasaron 24 horas desde el login, lanza excepción (sesión expirada).
    /// </summary>
    public TokenResult GetOrRotateToken(Guid userId)
    {
        var cacheKey = $"auth_token_info:{userId}";

        // Intentar obtener info del caché
        var cachedInfo = cacheService.Get<CachedTokenInfo>(cacheKey);

        if (cachedInfo is null)
        {
            // No hay info en caché - la sesión expiró completamente
            logger.LogWarning("Sesión expirada o inexistente para usuario: {UserId}", userId);
            throw new UnauthorizedAccessException("La sesión ha expirado. Por favor, inicie sesión nuevamente.");
        }

        // Verificar si ya pasaron 24 horas desde el login
        var maxSessionDuration = TimeSpan.FromMinutes(_tokenConfig.ExpirationMinutes);
        if (cachedInfo.IsExpired(maxSessionDuration))
        {
            // Limpiar caché y rechazar
            cacheService.Remove(cacheKey);
            logger.LogWarning("Sesión de 24 horas expirada para usuario: {UserId}", userId);
            throw new UnauthorizedAccessException("La sesión de 24 horas ha expirado. Por favor, inicie sesión nuevamente.");
        }

        // Verificar si el usuario sigue activo
        var user = userRepository.GetById(userId);
        if (user is null)
        {
            cacheService.Remove(cacheKey);
            throw new UnauthorizedAccessException(ErrorConstants.INVALID_CREDENTIALS);
        }

        if (!user.IsActive)
        {
            cacheService.Remove(cacheKey);
            throw new ForbiddenException(ErrorConstants.ACCOUNT_DISABLED);
        }

        // Calcular tiempo restante de la sesión
        var remainingTime = cachedInfo.GetRemainingTime(maxSessionDuration);

        // Verificar si debemos rotar el token (caché expiró o primera vez)
        var cacheExpiration = _tokenConfig.GetRandomCacheExpiration();
        var tokenCacheKey = $"auth_token:{userId}";
        var currentToken = cacheService.Get<string>(tokenCacheKey);

        string tokenToReturn;
        bool wasRotated = false;

        if (string.IsNullOrEmpty(currentToken))
        {
            // El token en caché expiró (pasaron 1-5 minutos) - ROTAR
            logger.LogInformation("Rotando token para usuario: {UserId} | Tiempo restante de sesión: {Remaining:hh\\:mm\\:ss}",
                userId, remainingTime);

            tokenToReturn = GenerateNewToken(cachedInfo);
            wasRotated = true;

            // Actualizar info de rotación
            cachedInfo.CurrentToken = tokenToReturn;
            cachedInfo.LastRotation = DateTime.UtcNow;

            logger.LogDebug("Token rotado. Nuevo token en caché por {CacheMin} minutos", cacheExpiration.TotalMinutes);
        }
        else
        {
            // El token sigue vigente en caché - devolver el mismo
            tokenToReturn = currentToken;
            logger.LogDebug("Token vigente recuperado del caché para usuario: {UserId}", userId);
        }

        // Guardar info actualizada en caché (1-5 minutos)
        cacheService.Set(cacheKey, cachedInfo, cacheExpiration);
        cacheService.Set(tokenCacheKey, tokenToReturn, cacheExpiration);

        return new TokenResult
        {
            Token = tokenToReturn,
            WasRotated = wasRotated,
            RemainingSessionTime = remainingTime
        };
    }

    /// <summary>
    /// Crea la información inicial del token al hacer login.
    /// </summary>
    private CachedTokenInfo CreateTokenInfo(Guid userId, string fullName, string role)
    {
        var tokenInfo = new CachedTokenInfo
        {
            UserId = userId,
            FullName = fullName,
            Role = role,
            LoginTime = DateTime.UtcNow,
            LastRotation = DateTime.UtcNow
        };

        // Generar el primer token JWT
        tokenInfo.CurrentToken = TokenHelper.GenerateJwtToken(userId, fullName, role, _tokenConfig);

        // Guardar en caché con expiración aleatoria inicial
        var cacheExpiration = _tokenConfig.GetRandomCacheExpiration();
        cacheService.Set($"auth_token_info:{userId}", tokenInfo, cacheExpiration);
        cacheService.Set($"auth_token:{userId}", tokenInfo.CurrentToken, cacheExpiration);

        logger.LogDebug("Token info creada y guardada en caché por {Minutes} minutos", cacheExpiration.TotalMinutes);

        return tokenInfo;
    }

    /// <summary>
    /// Genera un nuevo token JWT (rotación).
    /// </summary>
    private string GenerateNewToken(CachedTokenInfo cachedInfo)
    {
        return TokenHelper.GenerateJwtToken(
            cachedInfo.UserId,
            cachedInfo.FullName,
            cachedInfo.Role,
            _tokenConfig);
    }

    /// <summary>
    /// Obtiene el token actual sin rotar (para lectura pura).
    /// </summary>
    public string? GetCachedToken(Guid userId)
    {
        return cacheService.Get<string>($"auth_token:{userId}");
    }

    /// <summary>
    /// Invalida completamente la sesión del usuario (logout).
    /// </summary>
    public bool InvalidateTokenCache(Guid userId)
    {
        var infoKey = $"auth_token_info:{userId}";
        var tokenKey = $"auth_token:{userId}";

        cacheService.Remove(infoKey);
        var result = cacheService.Remove(tokenKey);

        if (result)
        {
            logger.LogInformation("Sesión invalidada para usuario: {UserId}", userId);
        }

        return result;
    }
}
