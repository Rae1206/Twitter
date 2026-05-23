using System.Security.Claims;
using Shared.Constants;

namespace WebApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? TryGetUserId(this ClaimsPrincipal? user)
    {
        var claim = user?.FindFirst(ClaimsConstants.USER_ID)?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : null;
    }

    public static Guid GetRequiredUserId(this ClaimsPrincipal? user)
    {
        return user.TryGetUserId()
            ?? throw new UnauthorizedAccessException(ResponseConstants.USER_NOT_EXISTS);
    }
}
