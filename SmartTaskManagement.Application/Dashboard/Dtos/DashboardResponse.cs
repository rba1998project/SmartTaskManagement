namespace SmartTaskManagement.Application.Dashboard.Dtos;

using SmartTaskManagement.Domain.Enums;

public sealed class DashboardResponse
{
    public int TotalProjects { get; init; }

    public int TotalTasks { get; init; }

    public Dictionary<TaskItemStatus, int> TasksByStatus { get; init; } = new();

    public Dictionary<TaskItemPriority, int> TasksByPriority { get; init; } = new();

    public int CompletedTasks { get; init; }

    public int PendingTasks { get; init; }

    public int UpcomingDueTasks { get; init; }
}
