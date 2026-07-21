namespace SmartTaskManagement.Application.Authentication.Dtos;

/// <summary>
/// Logout input. Revokes the supplied refresh token so it can no longer be exchanged.
/// </summary>
public sealed class LogoutRequestDto
{
    public string RefreshToken { get; init; } = string.Empty;
}
