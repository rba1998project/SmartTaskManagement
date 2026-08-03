using SmartTaskManagement.Application.Abstractions;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.Users.Dtos;

namespace SmartTaskManagement.Application.Users;

/// <summary>
/// Application use cases for user lookup and management.
/// </summary>
public sealed class UserService
{
    private readonly IUserRepository _users;
    private readonly IIdentityService _identity;
    private readonly ICurrentUserService _currentUser;

    public UserService(IUserRepository users, IIdentityService identity, ICurrentUserService currentUser)
    {
        _users = users;
        _identity = identity;
        _currentUser = currentUser;
    }

    public Task<PagedResult<UserLookupDto>> GetAssigneesAsync(
        AssigneeQueryRequestDto request,
        CancellationToken cancellationToken = default) =>
        _users.QueryAssigneesAsync(request, cancellationToken);

    public Task<PagedResult<UserListItemDto>> GetUsersAsync(
        UserQueryRequestDto request,
        CancellationToken cancellationToken = default) =>
        _users.QueryUsersAsync(request, cancellationToken);

    public async Task<Result> UpdateRoleAsync(
        Guid userId,
        string? roleName,
        CancellationToken cancellationToken = default)
    {
        if (userId == _currentUser.UserId)
            return Result.Failure(ErrorType.Forbidden, "You cannot change your own role.");

        return await _identity.UpdateUserRoleAsync(userId, roleName, cancellationToken);
    }
}
