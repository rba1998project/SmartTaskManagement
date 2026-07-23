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

        //totals
        var totalProjects = await _projects.CountVisibleAsync(teamMemberUserId, projectOwnerUserId, cancellationToken);
        var totalTasks = await _tasks.CountAsync(_ => true, assignedToUserId, projectOwnerUserId, cancellationToken);

        //breakdown of completed and pending tasks
        var tasksByStatus = await _tasks.CountByStatusAsync(assignedToUserId, projectOwnerUserId, cancellationToken);
        var tasksByPriority = await _tasks.CountByPriorityAsync(assignedToUserId, projectOwnerUserId, cancellationToken);

        //derived metrics
        var upcomingDueTasks = await _tasks.CountAsync(
            t => t.DueDate.HasValue && t.DueDate <= DateTime.UtcNow && (t.Status == TaskItemStatus.ToDo || t.Status == TaskItemStatus.InProgress),
            assignedToUserId, projectOwnerUserId, cancellationToken);

        
        var completedTasks = tasksByStatus.GetValueOrDefault(TaskItemStatus.Completed);
        var pendingTasks = tasksByStatus.GetValueOrDefault(TaskItemStatus.ToDo) + tasksByStatus.GetValueOrDefault(TaskItemStatus.InProgress);

        var response = new DashboardResponse
        {
            TotalProjects = totalProjects,
            TotalTasks = totalTasks,
            TasksByStatus = tasksByStatus,
            TasksByPriority = tasksByPriority,
            CompletedTasks = completedTasks,
            PendingTasks = pendingTasks,
            UpcomingDueTasks = upcomingDueTasks
        };

        return Result<DashboardResponse>.Success(response);
    }
}
