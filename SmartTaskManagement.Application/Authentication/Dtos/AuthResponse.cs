namespace SmartTaskManagement.Application.Authentication.Dtos;

/// <summary>
/// Tokens and identity returned by login and refresh. The refresh token is the raw
/// value — it is returned to the client once and only the hash is persisted server-side.
/// </summary>
public sealed class AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; init; }
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; init; }

    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? FullName { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}
