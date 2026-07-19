using Microsoft.AspNetCore.Identity;
using SmartTaskManagement.Application.Abstractions;
using SmartTaskManagement.Application.Authentication.Models;
using SmartTaskManagement.Application.Common;

namespace SmartTaskManagement.Infrastructure.Identity;

/// <summary>
/// ASP.NET Core Identity-backed implementation of <see cref="IIdentityService"/>. Uses
/// <c>UserManager</c> for user management and <c>CheckPasswordAsync</c> for credential checks
/// (no <c>SignInManager</c>). Maps Identity users onto the Application-neutral <see cref="AuthUser"/>.
/// </summary>
public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<AuthUser>> CreateUserAsync(string email, string password, string? fullName, string initialRole, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName
        };

        var created = await _userManager.CreateAsync(user, password);
        if (!created.Succeeded)
            return Result<AuthUser>.Failure(created.Errors.Select(e => e.Description));

        var roleAssigned = await _userManager.AddToRoleAsync(user, initialRole);
        if (!roleAssigned.Succeeded)
            return Result<AuthUser>.Failure(roleAssigned.Errors.Select(e => e.Description));

        return Result<AuthUser>.Success(ToAuthUser(user));
    }

    public async Task<AuthUser?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return null;

        if (!await _userManager.CheckPasswordAsync(user, password))
            return null;

        return ToAuthUser(user);
    }

    public async Task<AuthUser?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : ToAuthUser(user);
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Array.Empty<string>();

        var roles = await _userManager.GetRolesAsync(user);
        return roles.ToList();
    }

    private static AuthUser ToAuthUser(ApplicationUser user)
    {
        return new AuthUser(user.Id, user.Email ?? string.Empty , user.FullName);
    }
}
