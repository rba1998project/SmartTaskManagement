using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.API.Common;
using SmartTaskManagement.Application.Abstractions;
using SmartTaskManagement.Application.Authorization;
using SmartTaskManagement.Application.Users.Dtos;

namespace SmartTaskManagement.API.Controllers;

/// <summary>
/// Lightweight user directory endpoint for UI dropdowns and lookup flows.
/// Returns only non-sensitive fields; gated to users with task-assign permission.
/// </summary>
// Returns only non-sensitive fields; gated to users with task-assign permission.
[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IIdentityService _identityService;

    /// <summary>Initializes a new instance of <see cref="UsersController"/>.</summary>
    /// <param name="identityService">Identity lookup service.</param>
    public UsersController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    /// <summary>
    /// Returns a lightweight directory of users for assignment dropdowns.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of <see cref="UserLookupDto"/>.</returns>
    [HttpGet("lookup")]
    [Authorize(Policy = Permissions.TasksAssign)]
    public async Task<IActionResult> Lookup(CancellationToken cancellationToken)
    {
        var users = await _identityService.GetUserLookupAsync(cancellationToken);
        return Ok(ApiResponse.Ok(users));
    }

    /// <summary>
    /// Returns all users with their current role assignments.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _identityService.GetAllUsersAsync(cancellationToken);
        return Ok(ApiResponse.Ok(users));
    }

    /// <summary>
    /// Replaces the role assigned to the specified user.
    /// </summary>
    [HttpPut("{id:guid}/role")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateUserRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _identityService.UpdateUserRoleAsync(id, request?.RoleName, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Role update failed.");

        return Ok(ApiResponse.Ok<object?>(null));
    }
}
