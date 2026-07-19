namespace SmartTaskManagement.Application.Authentication.Models;

/// <summary>
/// A refresh token issued to a user — returned by both issue and rotate. Carries the owning
/// user id, the raw value handed to the client once (only its hash is persisted) and its
/// absolute UTC expiry.
/// </summary>
public sealed record IssuedRefreshToken(Guid UserId, string Token, DateTime ExpiresAt);
