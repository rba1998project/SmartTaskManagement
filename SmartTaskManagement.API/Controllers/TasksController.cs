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
/// per-project ownership and assigned-task visibility rules enforced inside <see cref="TaskService"/>.
/// </summary>
[ApiController]
[Authorize]
public sealed class TasksController : ControllerBase
{
    private readonly TaskService _taskService;

    /// <summary>Initializes a new instance of <see cref="TasksController"/>.</summary>
    /// <param name="taskService">Application task service.</param>
    public TasksController(TaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// Creates a new task inside the specified project.
    /// </summary>
    /// <param name="projectId">Parent project identifier.</param>
    /// <param name="request">Task creation input.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created <see cref="TaskResponseDto"/>.</returns>
    [Authorize(Policy = Permissions.TasksCreate)]
    [HttpPost("api/projects/{projectId:guid}/tasks")]
    public async Task<IActionResult> Create(Guid projectId, CreateTaskRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _taskService.CreateAsync(projectId, request, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Failed to create task.");

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id },
            ApiResponse.Ok(result.Value!, "Task created."));
    }

    /// <summary>
    /// Lists tasks for a specific project, with optional search, filtering, sorting, and pagination.
    /// </summary>
    /// <param name="projectId">Parent project identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of <see cref="TaskResponseDto"/>.</returns>
    [HttpGet("api/projects/{projectId:guid}/tasks")]
    public async Task<IActionResult> ListByProject(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await _taskService.ListByProjectAsync(projectId, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Project not found.");

        return Ok(ApiResponse.Ok(result.Value!));
    }

    /// <summary>
    /// Lists tasks visible to the current user across all projects, with optional search, filtering, sorting, and pagination.
    /// </summary>
    /// <param name="request">Query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of <see cref="TaskResponseDto"/>.</returns>
    [HttpGet("api/tasks")]
    public async Task<IActionResult> List([FromQuery] TaskQueryRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _taskService.ListAsync(request, cancellationToken);
        return Ok(ApiResponse.Ok(result.Value!));
    }

    /// <summary>
    /// Retrieves a single task by id.
    /// </summary>
    /// <param name="id">Task identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see cref="TaskResponseDto"/> if found and visible.</returns>
    [HttpGet("api/tasks/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _taskService.GetByIdAsync(id, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Task not found.");

        return Ok(ApiResponse.Ok(result.Value!));
    }

    /// <summary>
    /// Updates an existing task's details.
    /// </summary>
    /// <param name="id">Task identifier.</param>
    /// <param name="request">Update input.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated <see cref="TaskResponseDto"/>.</returns>
    [Authorize(Policy = Permissions.TasksUpdate)]
    [HttpPut("api/tasks/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateTaskRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _taskService.UpdateAsync(id, request, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Failed to update task.");

        return Ok(ApiResponse.Ok(result.Value!, "Task updated."));
    }

    /// <summary>
    /// Soft-deletes a task. The task is hidden from normal queries.
    /// </summary>
    /// <param name="id">Task identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Empty success response.</returns>
    [Authorize(Policy = Permissions.TasksDelete)]
    [HttpDelete("api/tasks/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _taskService.DeleteAsync(id, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Failed to delete task.");

        return Ok(ApiResponse.Ok<object?>(null, "Task deleted."));
    }

    /// <summary>
    /// Assigns a task to a user. Pass <c>null</c> to clear the assignment.
    /// </summary>
    /// <param name="id">Task identifier.</param>
    /// <param name="request">Assignee input.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated <see cref="TaskResponseDto"/>.</returns>
    [Authorize(Policy = Permissions.TasksAssign)]
    [HttpPut("api/tasks/{id:guid}/assignment")]
    public async Task<IActionResult> Assign(Guid id, AssignTaskRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _taskService.AssignAsync(id, request, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Failed to assign task.");

        return Ok(ApiResponse.Ok(result.Value!, "Task assignment updated."));
    }

    /// <summary>
    /// Changes a task's status. A Team Member may only change status on tasks assigned to them.
    /// </summary>
    /// <param name="id">Task identifier.</param>
    /// <param name="request">New status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated <see cref="TaskResponseDto"/>.</returns>
    [HttpPut("api/tasks/{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, UpdateTaskStatusRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _taskService.ChangeStatusAsync(id, request, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Failed to change task status.");

        return Ok(ApiResponse.Ok(result.Value!, "Task status updated."));
    }

    /// <summary>
    /// Improves a task description using the configured AI provider.
    /// Returns an improved description without modifying the task record.
    /// </summary>
    /// <param name="request">Raw description to improve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><see cref="ImproveTaskDescriptionResponse"/> with the improved text.</returns>
    [HttpPost("api/tasks/improve-description")]
    public async Task<IActionResult> ImproveDescription(ImproveTaskDescriptionRequest request, CancellationToken cancellationToken)
    {
        var result = await _taskService.ImproveDescriptionAsync(request.Description, cancellationToken);
        if (!result.Succeeded)
            return result.ToErrorResponse("Failed to improve description.");

        return Ok(ApiResponse.Ok(result.Value!, "Description improved."));
    }
}
