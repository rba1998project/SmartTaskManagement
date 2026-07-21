using SmartTaskManagement.Application.Abstractions;
using SmartTaskManagement.Application.Authentication.Dtos;
using SmartTaskManagement.Application.Authentication.Models;
using SmartTaskManagement.Application.Authorization;
using SmartTaskManagement.Application.Common;

namespace SmartTaskManagement.Application.Authentication;

/// <summary>
/// Orchestrates the authentication flows. Controllers stay thin by delegating here;
/// the Identity, JWT and refresh-token specifics live behind the injected abstractions.
/// </summary>
public sealed class AuthService
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthService(
        IIdentityService identityService,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenService refreshTokenService)
    {
        _identityService = identityService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenService = refreshTokenService;
    }

    /// <summary>
    /// Registers a new user and assigns the default Team Member role. Does not issue tokens —
    /// the client logs in separately.
    /// </summary>
    public async Task<Result> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var result = await _identityService.CreateUserAsync(
            request.Email,
            request.Password,
            request.FullName,
            RoleNames.TeamMember,
            cancellationToken);

        return result.Succeeded
            ? Result.Success()
            : Result.Failure(result.Errors);
    }

    /// <summary>
    /// Verifies credentials and, on success, issues an access token and a refresh token.
    /// Returns a generic failure on bad credentials to avoid user enumeration.
    /// </summary>
    public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _identityService.ValidateCredentialsAsync(request.Email, request.Password, cancellationToken);
        if (user is null)
            return Result<AuthResponseDto>.Failure("Invalid email or password.");

        var roles = await _identityService.GetRolesAsync(user.Id, cancellationToken);
        var permissions = await _identityService.GetPermissionsAsync(user.Id, cancellationToken);
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles, permissions);
        var refreshToken = await _refreshTokenService.IssueAsync(user.Id, cancellationToken);

        return Result<AuthResponseDto>.Success(BuildAuthResponse(user, roles, accessToken, refreshToken));
    }

    /// <summary>
    /// Exchanges a valid refresh token for a new access/refresh token pair, rotating out the
    /// presented token. Fails if the token is unknown, expired or already revoked.
    /// </summary>
    public async Task<Result<AuthResponseDto>> RefreshAsync(RefreshRequestDto request, CancellationToken cancellationToken = default)
    {
        var rotation = await _refreshTokenService.RotateAsync(request.RefreshToken, cancellationToken);
        if (!rotation.Succeeded)
            return Result<AuthResponseDto>.Failure(rotation.Errors);

        var rotated = rotation.Value!;

        var user = await _identityService.FindByIdAsync(rotated.UserId, cancellationToken);
        if (user is null)
            return Result<AuthResponseDto>.Failure("Invalid refresh token.");

        var roles = await _identityService.GetRolesAsync(user.Id, cancellationToken);
        var permissions = await _identityService.GetPermissionsAsync(user.Id, cancellationToken);
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles, permissions);

        return Result<AuthResponseDto>.Success(BuildAuthResponse(user, roles, accessToken, rotated));
    }

    /// <summary>
    /// Revokes the supplied refresh token. Idempotent — succeeds even for an unknown token.
    /// </summary>
    public async Task<Result> LogoutAsync(LogoutRequestDto request, CancellationToken cancellationToken = default)
    {
        await _refreshTokenService.RevokeAsync(request.RefreshToken, cancellationToken);
        return Result.Success();
    }

    private static AuthResponseDto BuildAuthResponse(
        AuthUser user,
        IReadOnlyList<string> roles,
        AccessToken accessToken,
        IssuedRefreshToken refreshToken) =>
        new()
        {
            AccessToken = accessToken.Token,
            AccessTokenExpiresAt = accessToken.ExpiresAt,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiresAt = refreshToken.ExpiresAt,
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Roles = roles,
        };
}
