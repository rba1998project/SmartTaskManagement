using SmartTaskManagement.Application.Authentication.Models;
using SmartTaskManagement.Application.Common;

namespace SmartTaskManagement.Application.Abstractions;

/// <summary>
/// Issues, rotates and revokes persisted refresh tokens. Implemented in Infrastructure,
/// which hashes the raw token (SHA-256), stores only the hash, and returns the raw value once.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Issues and persists a new refresh token for the user, returning the raw value and expiry.
    /// </summary>
    Task<IssuedRefreshToken> IssueAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the presented raw token, revokes it, and issues a replacement (rotation).
    /// The returned token carries the owning user id. Fails if the token is unknown, expired
    /// or already revoked.
    /// </summary>
    Task<Result<IssuedRefreshToken>> RotateAsync(string rawToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes the presented raw token if it exists. Idempotent — unknown or already-revoked
    /// tokens are treated as a no-op so logout never leaks token validity.
    /// </summary>
    Task RevokeAsync(string rawToken, CancellationToken cancellationToken = default);
}
