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

    public Task<GenericResponse<ResetPasswordResponse>> RequestPasswordReset(ResetPasswordRequest model)
    {
        throw new NotImplementedException();
    }

    public Task<GenericResponse<LoginAuthResponse>> VerifyOtpAndResetPassword(VerifyOtpRequest model)
    {
        throw new NotImplementedException();
    }
}
