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
    Task<GenericResponse<LoginAuthResponse>> Login(LoginAuthRequest model);
    Task<GenericResponse<LoginAuthResponse>> Renew(RenewAuthRequest model);
    Task<GenericResponse<ResetPasswordResponse>> RequestPasswordReset(ResetPasswordRequest model);
    Task<GenericResponse<LoginAuthResponse>> VerifyOtpAndResetPassword(VerifyOtpRequest model);
}
