namespace SmartTaskManagement.Application.Authentication.Dtos;

/// <summary>
/// Login input. On success the caller receives an <see cref="AuthResponse"/>.
/// </summary>
public sealed class LoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
