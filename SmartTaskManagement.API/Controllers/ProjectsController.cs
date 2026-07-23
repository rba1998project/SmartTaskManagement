using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.API.Common;
using SmartTaskManagement.Application.Authorization;
using SmartTaskManagement.Application.Projects;
using SmartTaskManagement.Application.Projects.Dtos;

namespace SmartTaskManagement.API.Controllers;

/// <summary>
/// Project endpoints. Reads are scoped by role: Admin sees all projects; Project Manager
/// sees only their own projects; Team Member sees only projects containing tasks assigned
/// to them. create/update/delete are gated to Admin and Project Manager, with per-project
/// ownership enforced inside <see cref="ProjectService"/>.
/// </summary>
// The controller stays thin: it delegates to the service and maps the returned Result onto HTTP.
[Authorize]
[ApiController]
[Route("api/projects")]
public sealed class ProjectsController : ControllerBase
{
    private readonly ProjectService _projectService;

    /// <summary>Initializes a new instance of <see cref="ProjectsController"/>.</summary>
    /// <param name="projectService">Application project service.</param>
    public ProjectsController(ProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    /// Lists projects visible to the current user, with optional search, sorting, and pagination.
    /// </summary>
    /// <param name="request">Query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of <see cref="ProjectResponseDto"/>.</returns>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] ProjectQueryRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _projectService.ListAsync(request, cancellationToken);
        return Ok(ApiResponse.Ok(result.Value!));
    }

    /// <summary>
    /// Retrieves a single project by id.
    /// </summary>
    /// <param name="id">Project identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see cref="ProjectResponseDto"/> if found.</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _projectService.GetByIdAsync(id, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Project not found.");

        return Ok(ApiResponse.Ok(result.Value!));
    }

    /// <summary>
    /// Creates a new project. The owner is set to the authenticated caller.
    /// </summary>
    /// <param name="request">Project creation input.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created <see cref="ProjectResponseDto"/>.</returns>
    [Authorize(Policy = Permissions.ProjectsCreate)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProjectRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _projectService.CreateAsync(request, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Failed to create project.");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id },
            ApiResponse.Ok(result.Value!, "Project created."));
    }

    /// <summary>
    /// Updates an existing project. Only the name and description are mutable.
    /// </summary>
    /// <param name="id">Project identifier.</param>
    /// <param name="request">Update input.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated <see cref="ProjectResponseDto"/>.</returns>
    [Authorize(Policy = Permissions.ProjectsUpdate)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateProjectRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _projectService.UpdateAsync(id, request, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Failed to update project.");

        return Ok(ApiResponse.Ok(result.Value!, "Project updated."));
    }

    /// <summary>
    /// Soft-deletes a project. The project and its tasks are hidden from normal queries.
    /// </summary>
    /// <param name="id">Project identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Empty success response.</returns>
    [Authorize(Policy = Permissions.ProjectsDelete)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _projectService.DeleteAsync(id, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Failed to delete project.");

        return Ok(ApiResponse.Ok<object?>(null, "Project deleted."));
    }
}
