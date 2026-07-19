using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartTaskManagement.Application.Abstractions;
using SmartTaskManagement.Application.Authentication.Models;

namespace SmartTaskManagement.Infrastructure.Authentication;

/// <summary>
/// Signs JWT access tokens with the User-Secrets signing key (HMAC-SHA256), using issuer,
/// audience and lifetime from <see cref="JwtOptions"/>. Emits sub/email/name claims plus one
/// role claim per assigned role to drive <c>[Authorize(Roles = ...)]</c>.
/// </summary>
public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;
    private readonly SigningCredentials _signingCredentials;

    public JwtTokenGenerator(IOptions<JwtOptions> options, string signingKey)
    {
        _options = options.Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public AccessToken GenerateAccessToken(AuthUser user, IReadOnlyList<string> roles)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (!string.IsNullOrWhiteSpace(user.FullName))
            claims.Add(new Claim(JwtRegisteredClaimNames.Name, user.FullName));

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: _signingCredentials);

        var raw = new JwtSecurityTokenHandler().WriteToken(token);
        return new AccessToken(raw, expiresAt);
    }
}
