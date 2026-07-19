using SmartTaskManagement.Application.Authentication.Models;

namespace SmartTaskManagement.Application.Abstractions;

/// <summary>
/// Issues signed JWT access tokens. Implemented in Infrastructure, where the signing key
/// (User Secrets / environment) and token descriptor live.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Builds a signed access token for the user carrying their id, email and role claims.
    /// </summary>
    AccessToken GenerateAccessToken(AuthUser user, IReadOnlyList<string> roles);
}
