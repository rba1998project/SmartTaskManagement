using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Domain.Entities;

namespace SmartTaskManagement.Application.Tasks.Dtos;

/// <summary>
/// Query parameters for the task list endpoint.
/// </summary>
public sealed class TaskQueryRequestDto
{
    /// <summary>Keyword search across <see cref="TaskResponseDto.Title"/> and <see cref="TaskResponseDto.Description"/>.</summary>
    public string? Search { get; init; }

    /// <summary>Filter by task status.</summary>
    public TaskItemStatus? Status { get; init; }

    /// <summary>Filter by task priority.</summary>
    public TaskItemPriority? Priority { get; init; }

    /// <summary>Filter by assigned user id.</summary>
    public Guid? AssignedToUserId { get; init; }

    /// <summary>
    /// Filter by due date. When supplied, only tasks due <b>on or before</b> this date are returned
    /// (inclusive). Useful for upcoming/overdue task views.
    /// </summary>
    public DateTime? DueDate { get; init; }

    /// <summary>Field to sort by.</summary>
    public TaskSortField SortField { get; init; } = TaskSortField.CreatedAt;

    /// <summary>Sort direction.</summary>
    public SortDirection SortDirection { get; init; } = SortDirection.Desc;

    /// <summary>Page number (1-based).</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Page size. Maximum 100.</summary>
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Fields available for sorting task list results.
/// </summary>
public enum TaskSortField
{
    /// <summary>Task title.</summary>
    Title = 0,

    /// <summary>Creation timestamp.</summary>
    CreatedAt = 1,

    /// <summary>Due date.</summary>
    DueDate = 2,

    /// <summary>Priority.</summary>
    Priority = 3,

    /// <summary>Status.</summary>
    Status = 4
}
