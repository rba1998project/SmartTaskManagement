using SmartTaskManagement.Application.Abstractions;
using SmartTaskManagement.Application.Authorization;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.Dashboard.Dtos;
using SmartTaskManagement.Domain.Enums;

namespace SmartTaskManagement.Application.Dashboard;

public sealed class DashboardService
{
    private readonly IProjectRepository _projects;
    private readonly ITaskRepository _tasks;
    private readonly ICurrentUserService _currentUser;

    public DashboardService(IProjectRepository projects, ITaskRepository tasks, ICurrentUserService currentUser)
    {
        _projects = projects;
        _tasks = tasks;
        _currentUser = currentUser;
    }

    public async Task<Result<DashboardResponse>> GetAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is not { } userId)
            return Result<DashboardResponse>.Failure(ErrorType.Forbidden, "Not authenticated.");

        Guid? teamMemberUserId = null;
        if (_currentUser.IsInRole(RoleNames.TeamMember) && !_currentUser.IsInRole(RoleNames.Admin))
            teamMemberUserId = userId;

        Guid? assignedToUserId = null;
        if (_currentUser.IsInRole(RoleNames.TeamMember) && !_currentUser.IsInRole(RoleNames.Admin))
            assignedToUserId = userId;

        Guid? projectOwnerUserId = null;
        if (_currentUser.IsInRole(RoleNames.ProjectManager) && !_currentUser.IsInRole(RoleNames.Admin))
            projectOwnerUserId = userId;

        var totalProjectsTask = _projects.CountVisibleAsync(teamMemberUserId, cancellationToken);
        var totalTasksTask = _tasks.CountAsync(_ => true, assignedToUserId, projectOwnerUserId, cancellationToken);
        var tasksByStatusTask = _tasks.CountByStatusAsync(assignedToUserId, projectOwnerUserId, cancellationToken);
        var tasksByPriorityTask = _tasks.CountByPriorityAsync(assignedToUserId, projectOwnerUserId, cancellationToken);
        var upcomingDueTasksTask = _tasks.CountAsync(
            t => t.DueDate.HasValue && t.DueDate <= DateTime.UtcNow && (t.Status == TaskItemStatus.ToDo || t.Status == TaskItemStatus.InProgress),
            assignedToUserId, projectOwnerUserId, cancellationToken);

        await Task.WhenAll(totalProjectsTask, totalTasksTask, tasksByStatusTask, tasksByPriorityTask, upcomingDueTasksTask);

        var tasksByStatus = tasksByStatusTask.Result;
        var tasksByPriority = tasksByPriorityTask.Result;

        var completedTasks = tasksByStatus.GetValueOrDefault(TaskItemStatus.Completed);
        var pendingTasks = tasksByStatus.GetValueOrDefault(TaskItemStatus.ToDo) + tasksByStatus.GetValueOrDefault(TaskItemStatus.InProgress);

        var response = new DashboardResponse
        {
            TotalProjects = totalProjectsTask.Result,
            TotalTasks = totalTasksTask.Result,
            TasksByStatus = tasksByStatus,
            TasksByPriority = tasksByPriority,
            CompletedTasks = completedTasks,
            PendingTasks = pendingTasks,
            UpcomingDueTasks = upcomingDueTasksTask.Result
        };

        return Result<DashboardResponse>.Success(response);
    }
}
