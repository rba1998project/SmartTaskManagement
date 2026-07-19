using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using SmartTaskManagement.Application.Abstractions;

namespace SmartTaskManagement.API.Authentication;

/// <summary>
/// Reads the authenticated caller from the current request's validated JWT claims.
/// The user id is taken from the standard name-identifier claim (the token's <c>sub</c>
/// maps to it by default), with a direct <c>sub</c> fallback for safety. Role checks rely
/// on the bearer scheme being configured with <see cref="ClaimTypes.Role"/> as the role claim.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User?.FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}
