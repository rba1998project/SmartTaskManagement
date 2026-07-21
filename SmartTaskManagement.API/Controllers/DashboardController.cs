using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.API.Common;
using SmartTaskManagement.Application.Dashboard;

namespace SmartTaskManagement.API.Controllers;

/// <summary>
/// Dashboard endpoint. Returns aggregate statistics for the current user,
/// with visibility rules enforced inside <see cref="DashboardService"/>.
/// The controller stays thin: it delegates to the service and maps the returned
/// <see cref="Application.Common.Result"/> onto HTTP.
/// </summary>
[ApiController]
[Route("api/dashboard")]
[Authorize]
public sealed class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetAsync(cancellationToken);
        return Ok(ApiResponse.Ok(result.Value!));
    }
}
