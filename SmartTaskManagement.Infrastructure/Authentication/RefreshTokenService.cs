using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartTaskManagement.Application.Abstractions;
using SmartTaskManagement.Application.Authentication.Models;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Infrastructure.Persistence;

namespace SmartTaskManagement.Infrastructure.Authentication;

/// <summary>
/// Issues, rotates and revokes persisted refresh tokens. Only the SHA-256 hash of the raw
/// token is stored; the raw value is returned to the caller once and never kept server-side.
/// </summary>
public sealed class RefreshTokenService : IRefreshTokenService
{
    private const int RawTokenBytes = 32;

    private readonly ApplicationDbContext _dbContext;
    private readonly JwtOptions _jwtOptions;

    public RefreshTokenService(ApplicationDbContext dbContext, IOptions<JwtOptions> jwtOptions)
    {
        _dbContext = dbContext;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<IssuedRefreshToken> IssueAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddDays(_jwtOptions.RefreshTokenDays);

        var rawToken = GenerateRawToken();
        var entity = new RefreshToken(userId, Hash(rawToken), now, expiresAt);

        _dbContext.RefreshTokens.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new IssuedRefreshToken(userId, rawToken, expiresAt);
    }

    public async Task<Result<IssuedRefreshToken>> RotateAsync(string rawToken, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var hash = Hash(rawToken);

        var existing = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == hash, cancellationToken);

        if (existing is null || !existing.IsActive(now))
            return Result<IssuedRefreshToken>.Failure("Invalid refresh token.");

        existing.Revoke(now);

        var expiresAt = now.AddDays(_jwtOptions.RefreshTokenDays);
        var newRawToken = GenerateRawToken();
        var replacement = new RefreshToken(existing.UserId, Hash(newRawToken), now, expiresAt);

        _dbContext.RefreshTokens.Add(replacement);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<IssuedRefreshToken>.Success(new IssuedRefreshToken(existing.UserId, newRawToken, expiresAt));
    }

    public async Task RevokeAsync(string rawToken, CancellationToken cancellationToken = default)
    {
        var hash = Hash(rawToken);

        var existing = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == hash, cancellationToken);

        if (existing is null)
            return;

        existing.Revoke(DateTime.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string GenerateRawToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(RawTokenBytes);
        return Convert.ToBase64String(bytes);
    }

    private static string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}
