using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.API.Common;
using SmartTaskManagement.Application.Authorization;
using SmartTaskManagement.Application.Projects;
using SmartTaskManagement.Application.Projects.Dtos;

namespace SmartTaskManagement.API.Controllers;

/// <summary>
/// Project endpoints. Reads (list and details) are open to any authenticated user;
/// create/update/delete are gated to Admin and Project Manager, with per-project
/// ownership enforced inside <see cref="ProjectService"/> (an Admin may touch any
/// project, a Project Manager only its own). The controller stays thin: it delegates
/// to the service and maps the returned <see cref="Application.Common.Result"/> onto HTTP.
/// </summary>
[ApiController]
[Route("api/projects")]
public sealed class ProjectsController : ControllerBase
{
    private readonly ProjectService _projectService;

    public ProjectsController(ProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _projectService.ListAsync(cancellationToken);
        return Ok(ApiResponse.Ok(result.Value!));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _projectService.GetByIdAsync(id, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Project not found.");

        return Ok(ApiResponse.Ok(result.Value!));
    }

    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.ProjectManager}")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var result = await _projectService.CreateAsync(request, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Failed to create project.");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id },
            ApiResponse.Ok(result.Value!, "Project created."));
    }

    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.ProjectManager}")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        var result = await _projectService.UpdateAsync(id, request, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Failed to update project.");

        return Ok(ApiResponse.Ok(result.Value!, "Project updated."));
    }

    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.ProjectManager}")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _projectService.DeleteAsync(id, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Failed to delete project.");

        return Ok(ApiResponse.Ok<object?>(null, "Project deleted."));
    }
}
