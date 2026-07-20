using SmartTaskManagement.Application.Abstractions;
using SmartTaskManagement.Application.Authorization;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.Tasks.Dtos;
using SmartTaskManagement.Domain.Entities;

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

    public TaskService(
        ITaskRepository tasks,
        IProjectRepository projects,
        IIdentityService identity,
        ICurrentUserService currentUser)
    {
        _tasks = tasks;
        _projects = projects;
        _identity = identity;
        _currentUser = currentUser;
    }

    public async Task<Result<TaskResponse>> CreateAsync(Guid projectId, CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var project = await _projects.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
            return Result<TaskResponse>.Failure(ErrorType.NotFound, "Project not found.");

        if (!CanManageProjectTasks(project))
            return Result<TaskResponse>.Failure(ErrorType.Forbidden, "You do not have permission to add tasks to this project.");

        var task = new TaskItem(projectId, request.Title, request.Description, request.Priority, request.DueDate, DateTime.UtcNow);
        await _tasks.AddAsync(task, cancellationToken);

        return Result<TaskResponse>.Success(Map(task));
    }

    public async Task<Result<TaskResponse>> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await _tasks.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return Result<TaskResponse>.Failure(ErrorType.NotFound, "Task not found.");

        var project = await _projects.GetByIdAsync(task.ProjectId, cancellationToken);
        if (project is null || !CanManageProjectTasks(project))
            return Result<TaskResponse>.Failure(ErrorType.Forbidden, "You do not have permission to modify this task.");

        task.UpdateDetails(request.Title, request.Description, request.Priority, request.DueDate, DateTime.UtcNow);
        await _tasks.UpdateAsync(task, cancellationToken);

        return Result<TaskResponse>.Success(Map(task));
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

    public async Task<Result<TaskResponse>> AssignAsync(Guid id, AssignTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await _tasks.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return Result<TaskResponse>.Failure(ErrorType.NotFound, "Task not found.");

        var project = await _projects.GetByIdAsync(task.ProjectId, cancellationToken);
        if (project is null || !CanManageProjectTasks(project))
            return Result<TaskResponse>.Failure(ErrorType.Forbidden, "You do not have permission to assign this task.");

        // A non-null assignee must reference an existing application user.
        if (request.AssignedToUserId is { } assigneeId)
        {
            var assignee = await _identity.FindByIdAsync(assigneeId, cancellationToken);
            if (assignee is null)
                return Result<TaskResponse>.Failure(ErrorType.Validation, "Assigned user does not exist.");
        }

        task.AssignTo(request.AssignedToUserId, DateTime.UtcNow);
        await _tasks.UpdateAsync(task, cancellationToken);

        return Result<TaskResponse>.Success(Map(task));
    }

    public async Task<Result<TaskResponse>> ChangeStatusAsync(Guid id, UpdateTaskStatusRequest request, CancellationToken cancellationToken = default)
    {
        var task = await _tasks.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return Result<TaskResponse>.Failure(ErrorType.NotFound, "Task not found.");

        var project = await _projects.GetByIdAsync(task.ProjectId, cancellationToken);
        if (project is null)
            return Result<TaskResponse>.Failure(ErrorType.NotFound, "Task not found.");

        // A Team Member may change status only on tasks assigned to them; Admin/PM (owner) always may.
        if (!CanManageProjectTasks(project) && !IsAssignedToCurrentUser(task))
            return Result<TaskResponse>.Failure(ErrorType.Forbidden, "You do not have permission to change this task's status.");

        task.ChangeStatus(request.Status, DateTime.UtcNow);
        await _tasks.UpdateAsync(task, cancellationToken);

        return Result<TaskResponse>.Success(Map(task));
    }

    public async Task<Result<TaskResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await _tasks.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return Result<TaskResponse>.Failure(ErrorType.NotFound, "Task not found.");

        var project = await _projects.GetByIdAsync(task.ProjectId, cancellationToken);
        if (project is null)
            return Result<TaskResponse>.Failure(ErrorType.NotFound, "Task not found.");

        // Team Members see only tasks assigned to them; Admin/PM (owner) see all tasks in the project.
        if (!CanManageProjectTasks(project) && !IsAssignedToCurrentUser(task))
            return Result<TaskResponse>.Failure(ErrorType.Forbidden, "You do not have permission to view this task.");

        return Result<TaskResponse>.Success(Map(task));
    }

    public async Task<Result<IReadOnlyList<TaskResponse>>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await _projects.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
            return Result<IReadOnlyList<TaskResponse>>.Failure(ErrorType.NotFound, "Project not found.");

        // Admin/PM (owner) see every task in the project; a Team Member sees only their assignments.
        Guid? assigneeFilter = CanManageProjectTasks(project) ? null : _currentUser.UserId;
        var tasks = await _tasks.ListByProjectAsync(projectId, assigneeFilter, cancellationToken);

        IReadOnlyList<TaskResponse> mapped = tasks.Select(Map).ToArray();
        return Result<IReadOnlyList<TaskResponse>>.Success(mapped);
    }

    // Admin may manage tasks in any project; a Project Manager only in projects they own. Team
    // Members never satisfy this — their limited access is handled by assignment checks instead.
    private bool CanManageProjectTasks(Project project) =>
        _currentUser.IsInRole(RoleNames.Admin)
        || (
            _currentUser.IsInRole(RoleNames.ProjectManager)
            && project.CreatedByUserId == _currentUser.UserId
           );

    private bool IsAssignedToCurrentUser(TaskItem task)
    {
        var userId = _currentUser.UserId;

        return userId.HasValue
            && task.AssignedToUserId == userId.Value;
    }

    private static TaskResponse Map(TaskItem task) => new()
    {
        Id = task.Id,
        ProjectId = task.ProjectId,
        Title = task.Title,
        Description = task.Description,
        Status = task.Status,
        Priority = task.Priority,
        DueDate = task.DueDate,
        AssignedToUserId = task.AssignedToUserId,
        CreatedAt = task.CreatedAt,
        UpdatedAt = task.UpdatedAt,
    };
}
