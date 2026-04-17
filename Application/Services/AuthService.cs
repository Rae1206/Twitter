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
    ICacheService cacheService) : IAuthService
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
}
