using Application.Helpers;
using Application.Interfaces.Services;
using Application.Models.Helpers;
using Application.Models.Requests.Auth;
using Application.Models.Responses;
using Application.Models.Responses.Auth;
using Twitter.Domain.Database.SqlServer;
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
    public GenericResponse<LoginAuthResponse> Login(LoginAuthRequest model)
    {
        var user = unitOfWork.Auth.GetByEmail(model.Email)
            ?? throw new UnauthorizedAccessException(ResponseConstants.AUTH_USER_OR_PASSWORD_NOT_FOUND);

        var passwordHash = unitOfWork.Users.GetPasswordHash(user.UserId);
        if (passwordHash is null || !BCrypt.Net.BCrypt.Verify(model.Password, passwordHash))
        {
            throw new UnauthorizedAccessException(ResponseConstants.AUTH_USER_OR_PASSWORD_NOT_FOUND);
        }

        var roles = unitOfWork.Roles.GetRolesByUserId(user.UserId)
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

    public GenericResponse<LoginAuthResponse> Renew(RenewAuthRequest model)
    {
        var findRefreshToken = cacheService.Get<RefreshToken>(
            CacheHelper.AuthRefreshTokenKey(model.RefreshToken)
        ) ?? throw new UnauthorizedAccessException(ResponseConstants.AUTH_REFRESH_TOKEN_NOT_FOUND);

        var user = unitOfWork.Users.GetById(findRefreshToken.UserId)
            ?? throw new UnauthorizedAccessException(ResponseConstants.USER_NOT_EXISTS);

        var roles = unitOfWork.Roles.GetRolesByUserId(user.UserId)
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
    /// Solicita recuperación de contraseña - envía OTP por email.
    /// </summary>
    public async Task<GenericResponse<ResetPasswordResponse>> RequestPasswordReset(ResetPasswordRequest model)
    {
        var user = unitOfWork.Auth.GetByEmail(model.Email);

        // Por seguridad, siempre devolvemos email enviado aunque el usuario no exista
        // Esto evita enumeración de usuarios

        if (user is not null)
        {
            var otp = OtpHelper.Generate();
            var cacheKey = OtpHelper.GetCacheKey(model.Email);

            // Almacenar OTP en cache por 15 minutos
            cacheService.Create(cacheKey, OtpHelper.Expiration, otp);

            // Enviar email con OTP
            _ = emailService.SendPasswordResetEmailAsync(model.Email, user.FullName, otp);
        }

        return ResponseHelper.Create(new ResetPasswordResponse { EmailSent = true });
    }

    /// <summary>
    /// Verifica OTP y cambia la contraseña.
    /// </summary>
    public async Task<GenericResponse<LoginAuthResponse>> VerifyOtpAndResetPassword(VerifyOtpRequest model)
    {
        var cacheKey = OtpHelper.GetCacheKey(model.Email);
        var storedOtp = cacheService.Get<string>(cacheKey);

        if (storedOtp is null)
        {
            throw new UnauthorizedAccessException("OTP expirado o inválido");
        }

        if (storedOtp != model.Otp)
        {
            throw new UnauthorizedAccessException("OTP incorrecto");
        }

        var user = unitOfWork.Auth.GetByEmail(model.Email)
            ?? throw new UnauthorizedAccessException(ResponseConstants.USER_NOT_EXISTS);

        // Cambiar contraseña
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        unitOfWork.Update(user);
        await unitOfWork.SaveChangesAsync();

        // Eliminar OTP usado
        cacheService.Delete(cacheKey);

        // Login automático después de cambiar contraseña
        var roles = unitOfWork.Roles.GetRolesByUserId(user.UserId)
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
}
