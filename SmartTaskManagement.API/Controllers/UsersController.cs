using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.API.Common;
using SmartTaskManagement.Application.Authorization;
using SmartTaskManagement.Application.Users;
using SmartTaskManagement.Application.Users.Dtos;

namespace SmartTaskManagement.API.Controllers;

/// <summary>
/// Users endpoint. Includes an assignee lookup for task assignment dropdowns
/// and admin-only user management endpoints.
/// </summary>
// Returns only non-sensitive fields; gated to users with task-assign permission.
[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly UserService _userService;

    /// <summary>Initializes a new instance of <see cref="UsersController"/>.</summary>
    /// <param name="userService">User application service.</param>
    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Returns users eligible to be assigned to tasks (Team Members only).
    /// </summary>
    /// <param name="request">Search and paging parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of <see cref="UserLookupDto"/>.</returns>
    [HttpGet("assignees")]
    [Authorize(Policy = Permissions.TasksAssign)]
    public async Task<IActionResult> GetAssignees([FromQuery] AssigneeQueryRequestDto request, CancellationToken cancellationToken)
    {
        var users = await _userService.GetAssigneesAsync(request, cancellationToken);
        return Ok(ApiResponse.Ok(users));
    }

    /// <summary>
    /// Returns a filtered, sorted, paged list of users with their current role assignments.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> GetAll([FromQuery] UserQueryRequestDto request, CancellationToken cancellationToken)
    {
        var users = await _userService.GetUsersAsync(request, cancellationToken);
        return Ok(ApiResponse.Ok(users));
    }

    /// <summary>
    /// Replaces the role assigned to the specified user.
    /// </summary>
    [HttpPut("{id:guid}/role")]
    [Authorize(Policy = Permissions.UsersManage)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateUserRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _userService.UpdateRoleAsync(id, request?.RoleName, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Role update failed.");

        return Ok(ApiResponse.Ok<object?>(null));
    }
}
