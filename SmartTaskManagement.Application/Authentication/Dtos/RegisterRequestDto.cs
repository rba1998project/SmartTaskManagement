namespace SmartTaskManagement.Application.Authentication.Dtos;

/// <summary>
/// Registration input. Registration creates the user and assigns the default
/// Team Member role only — it does not issue tokens.
/// </summary>
public sealed class RegisterRequestDto
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? FullName { get; init; }
}
