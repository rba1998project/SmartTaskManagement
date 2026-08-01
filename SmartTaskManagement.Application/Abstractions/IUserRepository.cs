using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.Users.Dtos;

namespace SmartTaskManagement.Application.Abstractions;

/// <summary>
/// Read access to user data used by user-management and assignment workflows.
/// </summary>
public interface IUserRepository
{
    Task<PagedResult<UserLookupDto>> QueryAssigneesAsync(
        AssigneeQueryRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserManagementDto>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
