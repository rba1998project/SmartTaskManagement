using SmartTaskManagement.Domain.Enums;

namespace SmartTaskManagement.Application.Tasks.Dtos;

/// <summary>
/// Input for changing a task's status. This is the only task mutation a Team Member may make,
/// and only on tasks assigned to them.
/// </summary>
public sealed class UpdateTaskStatusRequestDto
{
    public TaskItemStatus Status { get; init; }
}
