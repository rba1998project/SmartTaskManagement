using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.API.Common;
using SmartTaskManagement.Application.Authentication;
using SmartTaskManagement.Application.Authentication.Dtos;
using SmartTaskManagement.Application.Common;

namespace SmartTaskManagement.API.Controllers;

/// <summary>
/// Authentication endpoints. Register/login/refresh are anonymous; logout requires
/// authentication (covered by the global fallback policy). The controller stays thin —
/// all orchestration lives in <see cref="AuthService"/>.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Registration failed.");

        return Ok(ApiResponse.Ok<object?>(null, "Registration successful."));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Login failed.", ErrorType.Unauthorized);

        return Ok(ApiResponse.Ok(result.Value!, "Login successful."));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshAsync(request, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Token refresh failed.", ErrorType.Unauthorized);

        return Ok(ApiResponse.Ok(result.Value!, "Token refreshed."));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request, cancellationToken);
        return Ok(ApiResponse.Ok<object?>(null, "Logout successful."));
    }
}
