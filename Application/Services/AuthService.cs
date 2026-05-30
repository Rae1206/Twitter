using Application.Helpers;
using Application.Interfaces.Services;
using Application.Models.Helpers;
using Application.Models.Requests.Auth;
using Application.Models.Responses;
using Application.Models.Responses.Auth;
using Twitter.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Shared.Constants;

namespace Application.Services;

/// <summary>
/// Servicio de autenticación con JWT y refresh tokens.
/// </summary>
public class AuthService(
    IUnitOfWork unitOfWork,
    IConfiguration configuration,
    ICacheService cacheService,
    IEmailService emailService) : IAuthService
{
    /// <summary>
    /// Realiza la autenticación o inicio de sesión de un usuario verificando sus credenciales de email y contraseña.
    /// Genera y retorna un par de tokens: un JWT de corta duración y un refresh token.
    /// </summary>
    /// <param name="model">Modelo de solicitud con el email y contraseña ingresados.</param>
    /// <returns>Un sobre de respuesta que contiene el token JWT y el refresh token generados.</returns>
    public async Task<GenericResponse<LoginAuthResponse>> Login(LoginAuthRequest model)
    {
        var user = await unitOfWork.Auth.GetByEmailAsync(model.Email)
            ?? throw new UnauthorizedAccessException(ResponseConstants.AUTH_USER_OR_PASSWORD_NOT_FOUND);

        var passwordHash = await unitOfWork.Users.GetPasswordHashAsync(user.UserId);
        if (passwordHash is null || !BCrypt.Net.BCrypt.Verify(model.Password, passwordHash))
        {
            throw new UnauthorizedAccessException(ResponseConstants.AUTH_USER_OR_PASSWORD_NOT_FOUND);
        }

        var roles = (await unitOfWork.Roles.GetRolesByUserIdAsync(user.UserId))
            .Select(r => r.Name)
            .ToList();

        if (!roles.Any())
        {
            roles = new List<string> { RoleConstants.DefaultRole };
        }

        var token = TokenHelper.Create(user.UserId, roles, configuration, cacheService);
        var refreshToken = TokenHelper.CreateRefresh(user.UserId, configuration, cacheService);

        return ResponseHelper.Create(new LoginAuthResponse
        {
            Token = token,
            RefreshToken = refreshToken
        });
    }

    /// <summary>
    /// Renueva el token de acceso JWT utilizando un refresh token válido que esté guardado en caché.
    /// </summary>
    /// <param name="model">Modelo de solicitud que contiene el refresh token a verificar.</param>
    /// <returns>Un sobre de respuesta con un nuevo token JWT y un nuevo refresh token.</returns>
    public async Task<GenericResponse<LoginAuthResponse>> Renew(RenewAuthRequest model)
    {
        var findRefreshToken = cacheService.Get<RefreshToken>(
            CacheHelper.AuthRefreshTokenKey(model.RefreshToken)
        ) ?? throw new UnauthorizedAccessException(ResponseConstants.AUTH_REFRESH_TOKEN_NOT_FOUND);

        var user = await unitOfWork.Users.GetByIdAsync(findRefreshToken.UserId)
            ?? throw new UnauthorizedAccessException(ResponseConstants.USER_NOT_EXISTS);

        var roles = (await unitOfWork.Roles.GetRolesByUserIdAsync(user.UserId))
            .Select(r => r.Name)
            .ToList();

        if (!roles.Any())
        {
            roles = new List<string> { RoleConstants.DefaultRole };
        }

        var token = TokenHelper.Create(findRefreshToken.UserId, roles, configuration, cacheService);
        var refreshToken = TokenHelper.CreateRefresh(findRefreshToken.UserId, configuration, cacheService);

        cacheService.Delete(CacheHelper.AuthRefreshTokenKey(model.RefreshToken));

        return ResponseHelper.Create(new LoginAuthResponse
        {
            Token = token,
            RefreshToken = refreshToken
        });
    }

    /// <summary>
    /// Solicita de forma asíncrona la recuperación de la contraseña de un usuario mediante el envío de un correo electrónico.
    /// </summary>
    /// <param name="model">Modelo que contiene el correo electrónico del usuario.</param>
    /// <returns>El resultado del envío del correo de recuperación.</returns>
    public Task<GenericResponse<ResetPasswordResponse>> RequestPasswordReset(ResetPasswordRequest model)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Verifica el código OTP de un solo uso recibido por email y reestablece la contraseña por una nueva.
    /// </summary>
    /// <param name="model">Modelo con el email, OTP y la nueva contraseña del usuario.</param>
    /// <returns>Los nuevos tokens de acceso tras el cambio exitoso de contraseña.</returns>
    public Task<GenericResponse<LoginAuthResponse>> VerifyOtpAndResetPassword(VerifyOtpRequest model)
    {
        throw new NotImplementedException();
    }
}
