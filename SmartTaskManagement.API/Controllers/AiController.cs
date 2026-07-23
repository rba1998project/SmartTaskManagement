using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.API.Common;
using SmartTaskManagement.Application.Abstractions;

namespace SmartTaskManagement.API.Controllers;

/// <summary>
/// Lightweight endpoint indicating whether AI description improvement is available.
/// </summary>
// Returns a simple payload indicating whether the AI feature is configured.
[ApiController]
[AllowAnonymous]
public sealed class AiController : ControllerBase
{
    private readonly IAiStatusService _aiStatusService;

    /// <summary>Initializes a new instance of <see cref="AiController"/>.</summary>
    /// <param name="aiStatusService">Service that reports AI availability.</param>
    public AiController(IAiStatusService aiStatusService)
    {
        _aiStatusService = aiStatusService;
    }

    /// <summary>
    /// Returns a simple payload indicating whether the AI feature is configured.
    /// </summary>
    /// <returns>Anonymous object with <c>enabled</c> boolean.</returns>
    [HttpGet("api/ai/status")]
    public IActionResult GetStatus()
    {
        return Ok(ApiResponse.Ok(new { enabled = _aiStatusService.IsAvailable }));
    }
}
