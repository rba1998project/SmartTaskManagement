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
[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public UsersController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [HttpGet("lookup")]
    [Authorize(Policy = Permissions.TasksAssign)]
    public async Task<IActionResult> Lookup(CancellationToken cancellationToken)
    {
        var users = await _identityService.GetUserLookupAsync(cancellationToken);
        return Ok(ApiResponse.Ok(users));
    }
}
