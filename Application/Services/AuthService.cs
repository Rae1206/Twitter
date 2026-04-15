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
    IRoleRepository roleRepository,
    IOptions<TokenConfiguration> tokenOptions,
    ICacheService cacheService,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly TokenConfiguration _tokenConfig = tokenOptions.Value;

    public LoginResponse Login(LoginUserRequest model)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Intento de inicio de sesión con email: {Email}", model.Email);
        }

        var user = authRepository.GetByEmail(model.Email);

        if (user is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Credenciales inválidas para email: {Email}", model.Email);
            }
            throw new UnauthorizedAccessException(ErrorConstants.INVALID_CREDENTIALS);
        }

        if (!user.IsActive)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Intento de inicio de sesión con cuenta deshabilitada: {Email}", model.Email);
            }
            throw new ForbiddenException(ErrorConstants.ACCOUNT_DISABLED);
        }

        if (!authRepository.VerifyPassword(user.UserId, model.Password))
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Contraseña incorrecta para email: {Email}", model.Email);
            }
            throw new UnauthorizedAccessException(ErrorConstants.INVALID_CREDENTIALS);
        }

        // Obtener los roles del usuario desde UserRoles
        var roles = roleRepository.GetRolesByUserId(user.UserId)
            .Select(r => r.Name)
            .ToList();

        if (!roles.Any())
        {
            roles = new List<string> { RoleConstants.DefaultRole };
        }

        // Crear info de token con tiempo de login
        var tokenInfo = CreateTokenInfo(user.UserId, user.FullName, roles);

        // Calcular expiración del JWT (24 horas desde ahora)
        var jwtExpiration = _tokenConfig.GetExpirationDate();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Inicio de sesión exitoso para usuario: {Email} | ID: {UserId} | Roles: {Roles} | Sesión: 24h",
                model.Email, user.UserId, string.Join(", ", roles));
        }

        return new LoginResponse
        {
            Token = tokenInfo.CurrentToken,
            ExpiresAt = jwtExpiration,
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.FullName,
            Roles = roles
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
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Sesión expirada o inexistente para usuario: {UserId}", userId);
            }
            throw new UnauthorizedAccessException("La sesión ha expirado. Por favor, inicie sesión nuevamente.");
        }

        // Verificar si ya pasaron 24 horas desde el login
        var maxSessionDuration = TimeSpan.FromMinutes(_tokenConfig.ExpirationMinutes);
        if (cachedInfo.IsExpired(maxSessionDuration))
        {
            // Limpiar caché y rechazar
            cacheService.Remove(cacheKey);
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Sesión de 24 horas expirada para usuario: {UserId}", userId);
            }
            throw new UnauthorizedAccessException("La sesión de 24 horas ha expirado. Por favor, inicie sesión nuevamente.");
        }

        // Verificar si el usuario sigue activo y recargar roles desde BD
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

        // Recargar roles desde la base de datos para tener los más actuales
        var roles = roleRepository.GetRolesByUserId(userId)
            .Select(r => r.Name)
            .ToList();

        if (!roles.Any())
        {
            roles = new List<string> { RoleConstants.DefaultRole };
        }

        // Actualizar los roles en caché
        cachedInfo.Roles = roles;

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
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Rotando token para usuario: {UserId} | Tiempo restante de sesión: {Remaining:hh\\:mm\\:ss}",
                    userId, remainingTime);
            }

            tokenToReturn = GenerateNewToken(cachedInfo);
            wasRotated = true;

            // Actualizar info de rotación
            cachedInfo.CurrentToken = tokenToReturn;
            cachedInfo.LastRotation = DateTime.UtcNow;

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Token rotado. Nuevo token en caché por {CacheMin} minutos", cacheExpiration.TotalMinutes);
            }
        }
        else
        {
            // El token sigue vigente en caché - devolver el mismo
            tokenToReturn = currentToken;
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Token vigente recuperado del caché para usuario: {UserId}", userId);
            }
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
    private CachedTokenInfo CreateTokenInfo(Guid userId, string fullName, List<string> roles)
    {
        var tokenInfo = new CachedTokenInfo
        {
            UserId = userId,
            FullName = fullName,
            Roles = roles,
            LoginTime = DateTime.UtcNow,
            LastRotation = DateTime.UtcNow
        };

        // Generar el primer token JWT con múltiples roles
        tokenInfo.CurrentToken = TokenHelper.GenerateJwtToken(userId, fullName, roles, _tokenConfig);

        // Guardar en caché con expiración aleatoria inicial
        var cacheExpiration = _tokenConfig.GetRandomCacheExpiration();
        cacheService.Set($"auth_token_info:{userId}", tokenInfo, cacheExpiration);
        cacheService.Set($"auth_token:{userId}", tokenInfo.CurrentToken, cacheExpiration);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Token info creada y guardada en caché por {Minutes} minutos", cacheExpiration.TotalMinutes);
        }

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
            cachedInfo.Roles,
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

        if (result && logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Sesión invalidada para usuario: {UserId}", userId);
        }

        return result;
    }
}
