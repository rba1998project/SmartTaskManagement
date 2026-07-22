using SmartTaskManagement.Domain.Enums;

namespace SmartTaskManagement.Application.Tasks.Dtos;

/// <summary>
/// Input for creating a task. The owning project is taken from the route, not the body.
/// </summary>
public sealed class CreateTaskRequestDto
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public TaskItemStatus Status { get; init; }
    public TaskItemPriority Priority { get; init; }
    public DateTime? DueDate { get; init; }
}
