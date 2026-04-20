using Application.Models.Requests.Auth;
using Application.Models.Responses;
using Application.Models.Responses.Auth;

namespace Application.Interfaces.Services;

/// <summary>
/// Interfaz del servicio de autenticación.
/// Gestiona login/logout y tokens JWT con refresh tokens.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Inicia sesión con las credenciales proporcionadas.
    /// Devuelve un token JWT y un refresh token.
    /// </summary>
    /// <param name="model">Credenciales del usuario</param>
    /// <returns>LoginAuthResponse con token y refreshToken</returns>
    GenericResponse<LoginAuthResponse> Login(LoginAuthRequest model);

    /// <summary>
    /// Renueva el token de acceso usando un refresh token válido.
    /// </summary>
    /// <param name="model">Refresh token</param>
    /// <returns>Nuevo LoginAuthResponse con token y refreshToken</returns>
    GenericResponse<LoginAuthResponse> Renew(RenewAuthRequest model);

    /// <summary>
    /// Solicita recuperación de contraseña - envía OTP por email.
    /// </summary>
    Task<GenericResponse<ResetPasswordResponse>> RequestPasswordReset(ResetPasswordRequest model);

    /// <summary>
    /// Verifica OTP y cambia la contraseña.
    /// </summary>
    Task<GenericResponse<LoginAuthResponse>> VerifyOtpAndResetPassword(VerifyOtpRequest model);
}