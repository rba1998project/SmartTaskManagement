using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.API.Common;
using SmartTaskManagement.Application.Authentication;
using SmartTaskManagement.Application.Authentication.Dtos;
using SmartTaskManagement.Application.Common;

namespace SmartTaskManagement.API.Controllers;

/// <summary>
/// Authentication endpoints. Registration and login are anonymous; refresh is anonymous
/// but requires a valid refresh token; logout requires authentication.
/// </summary>
// Controller stays thin — all orchestration lives in AuthService.
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    /// <summary>Initializes a new instance of <see cref="AuthController"/>.</summary>
    /// <param name="authService">Application auth service.</param>
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Creates a new user account with the default Team Member role.
    /// Registration does not return tokens — authenticate afterwards to obtain them.
    /// </summary>
    /// <remarks>
    /// The password must meet the backend complexity rules (minimum 8 characters, upper, lower, digit, special).
    /// </remarks>
    /// <param name="request">Registration input.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Empty success response.</returns>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Registration failed.");

        return Ok(ApiResponse.Ok<object?>(null, "Registration successful."));
    }

    /// <summary>
    /// Authenticates a user and returns access and refresh tokens.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see cref="AuthResponseDto"/> containing tokens and user identity.</returns>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Login failed.", ErrorType.Unauthorized);

        return Ok(ApiResponse.Ok(result.Value!, "Login successful."));
    }

    /// <summary>
    /// Rotates an existing refresh token pair and returns a new access token
    /// and refresh token. The presented refresh token is revoked.
    /// </summary>
    /// <param name="request">Refresh token input.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see cref="AuthResponseDto"/> with new tokens.</returns>
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshAsync(request, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Token refresh failed.", ErrorType.Unauthorized);

        return Ok(ApiResponse.Ok(result.Value!, "Token refreshed."));
    }

    /// <summary>
    /// Revokes the supplied refresh token so it can no longer be exchanged.
    /// </summary>
    /// <param name="request">Logout input containing the refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Empty success response.</returns>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequestDto request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request, cancellationToken);
        return Ok(ApiResponse.Ok<object?>(null, "Logout successful."));
    }
}
