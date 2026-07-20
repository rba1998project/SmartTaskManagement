using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.API.Common;
using SmartTaskManagement.Application.Authorization;
using SmartTaskManagement.Application.Tasks;
using SmartTaskManagement.Application.Tasks.Dtos;

namespace SmartTaskManagement.API.Controllers;

/// <summary>
/// Task endpoints. Creating, editing, assigning and deleting are gated to Admin and Project
/// Manager; listing, viewing and status changes are open to any authenticated user, with the
/// per-project ownership and assigned-task visibility rules enforced inside
/// <see cref="TaskService"/>. The controller stays thin: it delegates to the service and maps
/// the returned <see cref="Application.Common.Result"/> onto HTTP.
/// </summary>
[ApiController]
[Authorize]
public sealed class TasksController : ControllerBase
{
    private readonly TaskService _taskService;

    public TasksController(TaskService taskService)
    {
        _taskService = taskService;
    }

    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.ProjectManager}")]
    [HttpPost("api/projects/{projectId:guid}/tasks")]
    public async Task<IActionResult> Create(Guid projectId, CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.CreateAsync(projectId, request, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Failed to create task.");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id },
            ApiResponse.Ok(result.Value!, "Task created."));
    }

    [HttpGet("api/projects/{projectId:guid}/tasks")]
    public async Task<IActionResult> ListByProject(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await _taskService.ListByProjectAsync(projectId, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Project not found.");

        return Ok(ApiResponse.Ok(result.Value!));
    }

    [HttpGet("api/tasks/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _taskService.GetByIdAsync(id, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Task not found.");

        return Ok(ApiResponse.Ok(result.Value!));
    }

    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.ProjectManager}")]
    [HttpPut("api/tasks/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.UpdateAsync(id, request, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Failed to update task.");

        return Ok(ApiResponse.Ok(result.Value!, "Task updated."));
    }

    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.ProjectManager}")]
    [HttpDelete("api/tasks/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _taskService.DeleteAsync(id, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Failed to delete task.");

        return Ok(ApiResponse.Ok<object?>(null, "Task deleted."));
    }

    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.ProjectManager}")]
    [HttpPut("api/tasks/{id:guid}/assignment")]
    public async Task<IActionResult> Assign(Guid id, AssignTaskRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.AssignAsync(id, request, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Failed to assign task.");

        return Ok(ApiResponse.Ok(result.Value!, "Task assignment updated."));
    }

    // Open to any authenticated user; a Team Member may change status only on tasks assigned to
    // them, which TaskService enforces.
    [HttpPut("api/tasks/{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, UpdateTaskStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.ChangeStatusAsync(id, request, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Failed to change task status.");

        return Ok(ApiResponse.Ok(result.Value!, "Task status updated."));
    }
}
