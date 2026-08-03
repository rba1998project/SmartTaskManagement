using SmartTaskManagement.Domain.Enums;

namespace SmartTaskManagement.Application.Tasks.Dtos;

/// <summary>
/// Input for updating a task's editable details. Assignment changes through its own endpoint;
/// Admin and Project Manager status edits are audited by the application service.
/// </summary>
public sealed class UpdateTaskRequestDto
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public TaskItemStatus? Status { get; init; }
    public TaskItemPriority Priority { get; init; }
    public DateTime? DueDate { get; init; }
}
