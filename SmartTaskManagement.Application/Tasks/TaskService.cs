using SmartTaskManagement.Application.Abstractions;
using SmartTaskManagement.Application.Authorization;
using SmartTaskManagement.Application.Authentication.Models;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.Tasks.Dtos;
using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Domain.Enums;

namespace SmartTaskManagement.Application.Tasks;

/// <summary>
/// Task use cases: create, update, delete, assign, change status, details, list. Controllers
/// stay thin by delegating here. Access rules live in this layer (not the API):
/// <list type="bullet">
///   <item><description>Admin: full access to any task.</description></item>
///   <item><description>Project Manager: manage/assign/change-status only for tasks in projects they own.</description></item>
///   <item><description>Team Member: view tasks assigned to them, and change status only on those tasks. Cannot create, edit details, assign, or delete.</description></item>
/// </list>
/// Role-gating of who may reach a mutating endpoint at all is enforced at the API with
/// [Authorize(Roles = ...)]; the ownership/assignment checks here are the second gate.
/// Expected application/use-case failures are returned as categorized <see cref="Result"/> values.
/// Domain invariant violations are guarded by the Domain entity.
/// </summary>
public sealed class TaskService
{
    private readonly ITaskRepository _tasks;
    private readonly IProjectRepository _projects;
    private readonly IIdentityService _identity;
    private readonly ICurrentUserService _currentUser;
    private readonly ITaskAiService _taskAiService;

    public TaskService(
        ITaskRepository tasks,
        IProjectRepository projects,
        IIdentityService identity,
        ICurrentUserService currentUser,
        ITaskAiService taskAiService)
    {
        _tasks = tasks;
        _projects = projects;
        _identity = identity;
        _currentUser = currentUser;
        _taskAiService = taskAiService;
    }

    public async Task<Result<TaskResponseDto>> CreateAsync(Guid projectId, CreateTaskRequestDto request, CancellationToken cancellationToken = default)
    {
        var project = await _projects.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
            return Result<TaskResponseDto>.Failure(ErrorType.NotFound, "Project not found.");

        if (!CanManageProjectTasks(project))
            return Result<TaskResponseDto>.Failure(ErrorType.Forbidden, "You do not have permission to add tasks to this project.");

        var task = new TaskItem(projectId, request.Title, request.Description, request.Priority, request.DueDate, DateTime.UtcNow);
        task.ChangeStatus(request.Status, DateTime.UtcNow);

        await _tasks.AddAsync(task, cancellationToken);

        return Result<TaskResponseDto>.Success(Map(task, project.Name, null));
    }

    public async Task<Result<TaskResponseDto>> UpdateAsync(Guid id, UpdateTaskRequestDto request, CancellationToken cancellationToken = default)
    {
        var task = await _tasks.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return Result<TaskResponseDto>.Failure(ErrorType.NotFound, "Task not found.");

        var project = await _projects.GetByIdAsync(task.ProjectId, cancellationToken);
        if (project is null || !CanManageProjectTasks(project))
            return Result<TaskResponseDto>.Failure(ErrorType.Forbidden, "You do not have permission to modify this task.");

        task.UpdateDetails(request.Title, request.Description, request.Priority, request.DueDate, DateTime.UtcNow);
        if (request.Status.HasValue)
        {
            task.ChangeStatus(request.Status.Value, DateTime.UtcNow);
        }
        await _tasks.UpdateAsync(task, cancellationToken);

        return Result<TaskResponseDto>.Success(Map(task, project.Name, null));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await _tasks.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return Result.Failure(ErrorType.NotFound, "Task not found.");

        var project = await _projects.GetByIdAsync(task.ProjectId, cancellationToken);
        if (project is null || !CanManageProjectTasks(project))
            return Result.Failure(ErrorType.Forbidden, "You do not have permission to delete this task.");

        await _tasks.RemoveAsync(task, cancellationToken);
        return Result.Success();
    }

    public async Task<Result<TaskResponseDto>> AssignAsync(Guid id, AssignTaskRequestDto request, CancellationToken cancellationToken = default)
    {
        var task = await _tasks.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return Result<TaskResponseDto>.Failure(ErrorType.NotFound, "Task not found.");

        var project = await _projects.GetByIdAsync(task.ProjectId, cancellationToken);
        if (project is null || !CanManageProjectTasks(project))
            return Result<TaskResponseDto>.Failure(ErrorType.Forbidden, "You do not have permission to assign this task.");

        AuthUser? assignee = null;
        if (request.AssignedToUserId is { } assigneeId)
        {
            assignee = await _identity.FindByIdAsync(assigneeId, cancellationToken);

            if (assignee is null)
                return Result<TaskResponseDto>.Failure(ErrorType.NotFound, "Assigned user does not exist.");
        }

        task.AssignTo(request.AssignedToUserId, DateTime.UtcNow);
        await _tasks.UpdateAsync(task, cancellationToken);

        return Result<TaskResponseDto>.Success(Map(task, project.Name, assignee?.FullName));
    }

    public async Task<Result<TaskResponseDto>> ChangeStatusAsync(Guid id, UpdateTaskStatusRequestDto request, CancellationToken cancellationToken = default)
    {
        var task = await _tasks.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return Result<TaskResponseDto>.Failure(ErrorType.NotFound, "Task not found.");

        var project = await _projects.GetByIdAsync(task.ProjectId, cancellationToken);
        if (project is null)
            return Result<TaskResponseDto>.Failure(ErrorType.NotFound, "Task not found.");

        // A Team Member may change status only on tasks assigned to them; Admin/PM (owner) always may.
        if (!CanManageProjectTasks(project) && !IsAssignedToCurrentUser(task))
            return Result<TaskResponseDto>.Failure(ErrorType.Forbidden, "You do not have permission to change this task's status.");

        task.ChangeStatus(request.Status, DateTime.UtcNow);
        await _tasks.UpdateAsync(task, cancellationToken);

        var assigneeName = task.AssignedToUserId is not null ? (await _identity.FindByIdAsync(task.AssignedToUserId.Value, cancellationToken))?.FullName : null;
        return Result<TaskResponseDto>.Success(Map(task, project.Name, assigneeName));
    }

    public async Task<Result<TaskResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await _tasks.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return Result<TaskResponseDto>.Failure(ErrorType.NotFound, "Task not found.");

        var project = await _projects.GetByIdAsync(task.ProjectId, cancellationToken);
        if (project is null)
            return Result<TaskResponseDto>.Failure(ErrorType.NotFound, "Task not found.");

        // Team Members see only tasks assigned to them; Admin/PM (owner) see all tasks in the project.
        if (!CanManageProjectTasks(project) && !IsAssignedToCurrentUser(task))
            return Result<TaskResponseDto>.Failure(ErrorType.Forbidden, "You do not have permission to view this task.");

        var assigneeName = task.AssignedToUserId is not null ? (await _identity.FindByIdAsync(task.AssignedToUserId.Value, cancellationToken))?.FullName : null;
        return Result<TaskResponseDto>.Success(Map(task, project.Name, assigneeName));
    }

    public async Task<Result<IReadOnlyList<TaskResponseDto>>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await _projects.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
            return Result<IReadOnlyList<TaskResponseDto>>.Failure(ErrorType.NotFound, "Project not found.");

        // Admin/PM (owner) see every task in the project; a Team Member sees only their assignments.
        Guid? assigneeFilter = CanManageProjectTasks(project) ? null : _currentUser.UserId;
        var tasks = await _tasks.ListByProjectAsync(projectId, assigneeFilter, cancellationToken);

        var userIds = tasks.Select(t => t.AssignedToUserId).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToArray();
        var users = await _identity.FindByIdsAsync(userIds, cancellationToken);
        var userMap = users.ToDictionary(u => u.Id, u => u.FullName);

        var mapped = tasks.Select(t => Map(t, project.Name, t.AssignedToUserId is { } assigneeId && userMap.TryGetValue(assigneeId, out var assigneeName) ? assigneeName : null)).ToArray();
        return Result<IReadOnlyList<TaskResponseDto>>.Success(mapped);
    }

    public async Task<Result<PagedResult<TaskResponseDto>>> ListAsync(TaskQueryRequestDto request, CancellationToken cancellationToken = default)
    {
        // Team Member: always restricted to their own assignments, regardless of request value.
        // Project Manager: scoped to projects they own. Admin: no scoping.
        var assignedToUserId = _currentUser.IsInRole(RoleNames.TeamMember) ? _currentUser.UserId : request.AssignedToUserId;
        Guid? projectOwnerUserId = null;

        if (_currentUser.IsInRole(RoleNames.ProjectManager) && !_currentUser.IsInRole(RoleNames.Admin))
            projectOwnerUserId = _currentUser.UserId;

        var pagedResult = await _tasks.QueryAsync(request, assignedToUserId, projectOwnerUserId, cancellationToken);

        var projectIds = pagedResult.Items.Select(t => t.ProjectId).Distinct().ToArray();
        var projects = await _projects.GetByIdsAsync(projectIds, cancellationToken);
        var projectMap = projects.ToDictionary(p => p.Id, p => p.Name);

        var userIds = pagedResult.Items.Select(t => t.AssignedToUserId).Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToArray();
        var users = await _identity.FindByIdsAsync(userIds, cancellationToken);
        var userMap = users.ToDictionary(u => u.Id, u => u.FullName);

        var mapped = new PagedResult<TaskResponseDto>(
            pagedResult.Items.Select(t =>
            {
                var projectName = projectMap.TryGetValue(t.ProjectId, out var pName) ? pName : string.Empty;
                var assigneeName = t.AssignedToUserId is { } aId && userMap.TryGetValue(aId, out var uName) ? uName : null;
                return Map(t, projectName, assigneeName);
            }).ToArray(),
            pagedResult.TotalCount,
            pagedResult.PageNumber,
            pagedResult.PageSize);

        return Result<PagedResult<TaskResponseDto>>.Success(mapped);
    }

    public async Task<Result<ImproveTaskDescriptionResponse>> ImproveDescriptionAsync(string description, CancellationToken cancellationToken = default)
    {
        var result = await _taskAiService.ImproveDescriptionAsync(description, cancellationToken);
        if (!result.Succeeded)
            return Result<ImproveTaskDescriptionResponse>.Failure(result.Errors);

        return Result<ImproveTaskDescriptionResponse>.Success(new ImproveTaskDescriptionResponse
        {
            ImprovedDescription = result.Value!
        });
    }

    // Admin may manage tasks in any project; a Project Manager only in projects they own. Team
    // Members never satisfy this — their limited access is handled by assignment checks instead.
    private bool CanManageProjectTasks(Project project)
    {
        return _currentUser.IsInRole(RoleNames.Admin)
            || (    _currentUser.IsInRole(RoleNames.ProjectManager)
                    && project.CreatedByUserId == _currentUser.UserId
                );
    }

    private bool IsAssignedToCurrentUser(TaskItem task)
    {
        var userId = _currentUser.UserId;

        return userId.HasValue
            && task.AssignedToUserId == userId.Value;
    }

    private static TaskResponseDto Map(TaskItem task, string projectName, string? assignedToUserName = null) => new()
    {
        Id = task.Id,
        ProjectId = task.ProjectId,
        ProjectName = projectName,
        Title = task.Title,
        Description = task.Description,
        Status = task.Status,
        Priority = task.Priority,
        DueDate = task.DueDate,
        AssignedToUserId = task.AssignedToUserId,
        AssignedToUserName = assignedToUserName,
        CreatedAt = task.CreatedAt,
        UpdatedAt = task.UpdatedAt,
    };
}
