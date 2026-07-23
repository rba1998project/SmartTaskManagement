using SmartTaskManagement.Application.Authentication.Models;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.Users.Dtos;

namespace SmartTaskManagement.Application.Abstractions;

/// <summary>
/// Application-facing wrapper over ASP.NET Core Identity user management. Implemented in
/// Infrastructure with <c>UserManager</c> (and <c>CheckPasswordAsync</c> for credential checks —
/// no <c>SignInManager</c>). Keeps the Identity types out of the Application layer.
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Creates a user with the given credentials and assigns <paramref name="initialRole"/>.
    /// Fails (without creating anything usable) if the email is taken or the password is rejected.
    /// </summary>
    Task<Result<AuthUser>> CreateUserAsync(string email, string password, string? fullName, string initialRole, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the user if the email exists and the password is correct; otherwise <c>null</c>.
    /// Does not distinguish "no such user" from "wrong password" to avoid user enumeration.
    /// </summary>
    Task<AuthUser?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the user by id, or <c>null</c> if not found.
    /// </summary>
    Task<AuthUser?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the matching users for the given ids, or an empty list if none match.
    /// Used for batch display-name lookups to avoid N+1 queries.
    /// </summary>
    Task<IReadOnlyList<AuthUser>> FindByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a lightweight list of users for directory/selection UIs.
    /// Returns only id, full name and email; excludes sensitive Identity fields.
    /// </summary>
    Task<IReadOnlyList<UserLookupDto>> GetUserLookupAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all users with their current role assignments.
    /// </summary>
    Task<IReadOnlyList<UserManagementDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the role assigned to the specified user.
    /// Passing <paramref name="roleName"/> as <c>null</c> or empty removes all roles.
    /// </summary>
    Task<Result> UpdateUserRoleAsync(Guid userId, string? roleName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the role names assigned to the user (empty if none).
    /// </summary>
    Task<IReadOnlyList<string>> GetRolesAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the distinct permission keys granted to the user through its roles (empty if none).
    /// These become the permission claims carried in the access token.
    /// </summary>
    Task<IReadOnlyList<string>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
