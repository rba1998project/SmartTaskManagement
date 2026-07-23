using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.API.Common;
using SmartTaskManagement.Application.Dashboard;

namespace SmartTaskManagement.API.Controllers;

/// <summary>
/// Dashboard endpoint. Returns aggregate statistics and recent activity for the current user.
/// Visibility rules are enforced inside <see cref="DashboardService"/>.
/// </summary>
// The controller stays thin: it delegates to the service and maps the returned Result onto HTTP.
[ApiController]
[Route("api/dashboard")]
[Authorize]
public sealed class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    /// <summary>Initializes a new instance of <see cref="DashboardController"/>.</summary>
    /// <param name="dashboardService">Application dashboard service.</param>
    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Retrieves dashboard statistics for the current user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Aggregated dashboard data.</returns>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetAsync(cancellationToken);
        return Ok(ApiResponse.Ok(result.Value!));
    }
}
