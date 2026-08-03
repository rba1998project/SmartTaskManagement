namespace SmartTaskManagement.Application.Authentication.Dtos;

/// <summary>
/// Self-service password change input. The target user is always taken from the
/// authenticated request and is never supplied by the client.
/// </summary>
public sealed class ChangePasswordRequestDto
{
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}
