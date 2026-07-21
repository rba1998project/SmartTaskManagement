namespace SmartTaskManagement.Application.Authentication.Dtos;

/// <summary>
/// Login input. On success the caller receives an <see cref="AuthResponseDto"/>.
/// </summary>
public sealed class LoginRequestDto
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
