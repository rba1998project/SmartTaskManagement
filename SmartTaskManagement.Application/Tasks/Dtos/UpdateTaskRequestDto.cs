using SmartTaskManagement.Domain.Enums;

namespace SmartTaskManagement.Application.Tasks.Dtos;

/// <summary>
/// Input for updating a task's editable details. Status and assignment change through their
/// own endpoints; the owning project never changes through an update.
/// </summary>
public sealed class UpdateTaskRequestDto
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public TaskItemPriority Priority { get; init; }
    public DateTime? DueDate { get; init; }
}
