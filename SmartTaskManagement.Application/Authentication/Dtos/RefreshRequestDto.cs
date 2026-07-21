namespace SmartTaskManagement.Application.Authentication.Dtos;

/// <summary>
/// Refresh input. The raw refresh token issued at login (or the previous refresh)
/// is exchanged for a new access/refresh token pair; the presented token is rotated out.
/// </summary>
public sealed class RefreshRequestDto
{
    public string RefreshToken { get; init; } = string.Empty;
}
