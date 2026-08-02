using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.Users.Dtos;

namespace SmartTaskManagement.Application.Abstractions;

/// <summary>
/// Read access to user data used by user list and assignment workflows.
/// </summary>
public interface IUserRepository
{
    Task<PagedResult<UserLookupDto>> QueryAssigneesAsync(
        AssigneeQueryRequestDto request,
        CancellationToken cancellationToken = default);

    Task<PagedResult<UserListItemDto>> QueryUsersAsync(
        UserQueryRequestDto request,
        CancellationToken cancellationToken = default);
}
