using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartTaskManagement.Application.Abstractions;
using SmartTaskManagement.Application.Authentication.Models;
using SmartTaskManagement.Application.Authorization;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.Users.Dtos;

namespace SmartTaskManagement.Infrastructure.Identity;

/// <summary>
/// ASP.NET Core Identity-backed implementation of <see cref="IIdentityService"/>. Uses
/// <c>UserManager</c> for user management and <c>CheckPasswordAsync</c> for credential checks
/// (no <c>SignInManager</c>). Maps Identity users onto the Application-neutral <see cref="AuthUser"/>.
/// </summary>
public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public IdentityService(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
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

    public async Task<IReadOnlyList<AuthUser>> FindByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var ids = userIds.Distinct().ToArray();
        if (ids.Length == 0)
            return Array.Empty<AuthUser>();

        var users = await _userManager.Users
            .Where(u => ids.Contains(u.Id))
            .ToListAsync(cancellationToken);

        return users.Select(ToAuthUser).ToArray();
    }

    public async Task<IReadOnlyList<UserLookupDto>> GetAssigneesAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.GetUsersInRoleAsync(RoleNames.TeamMember);

        return users
            .Select(u => new UserLookupDto(u.Id, u.FullName ?? string.Empty, u.Email ?? string.Empty))
            .ToList();
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Array.Empty<string>();

        var roles = await _userManager.GetRolesAsync(user);
        return roles.ToList();
    }

    public async Task<IReadOnlyList<UserManagementDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .OrderBy(u => u.Email)
            .ToListAsync(cancellationToken);

        var result = new List<UserManagementDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();
            result.Add(new UserManagementDto(user.Id, user.Email ?? string.Empty, user.FullName, role ?? string.Empty));
        }

        return result;
    }

    public async Task<Result> UpdateUserRoleAsync(Guid userId, string? roleName, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Result.Failure("User not found.");

        var currentRoles = await _userManager.GetRolesAsync(user);
        var requested = new[] { roleName }.Where(r => !string.IsNullOrWhiteSpace(r)).ToArray()!;

        var rolesToRemove = currentRoles.Except(requested, StringComparer.OrdinalIgnoreCase).Cast<string>().ToList();
        var rolesToAdd = requested.Except(currentRoles, StringComparer.OrdinalIgnoreCase).Cast<string>().ToList();

        if (rolesToRemove.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
                return Result.Failure(removeResult.Errors.Select(e => e.Description));
        }

        if (rolesToAdd.Count > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
                return Result.Failure(addResult.Errors.Select(e => e.Description));
        }

        return Result.Success();
    }

    public async Task<IReadOnlyList<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Array.Empty<string>();

        var roleNames = await _userManager.GetRolesAsync(user);

        var permissions = new HashSet<string>();
        foreach (var roleName in roleNames)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is null)
                continue;

            var roleClaims = await _roleManager.GetClaimsAsync(role);
            foreach (var claim in roleClaims)
            {
                if (claim.Type == Permissions.ClaimType)
                    permissions.Add(claim.Value);
            }
        }

        return permissions.ToList();
    }

    private static AuthUser ToAuthUser(ApplicationUser user)
    {
        return new AuthUser(user.Id, user.Email ?? string.Empty , user.FullName);
    }
}
