namespace SmartTaskManagement.Application.Authentication.Models;

/// <summary>
/// A signed JWT access token and its absolute UTC expiry.
/// </summary>
public sealed record AccessToken(string Token, DateTime ExpiresAt);
