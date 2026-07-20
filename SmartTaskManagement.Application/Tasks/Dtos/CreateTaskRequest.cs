using SmartTaskManagement.Domain.Entities;

namespace SmartTaskManagement.Application.Tasks.Dtos;

/// <summary>
/// Input for creating a task. The owning project is taken from the route, not the body.
/// A new task starts in <see cref="TaskItemStatus.ToDo"/>; assignment is done separately.
/// </summary>
public sealed class CreateTaskRequest
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public TaskItemPriority Priority { get; init; }
    public DateTime? DueDate { get; init; }
}
