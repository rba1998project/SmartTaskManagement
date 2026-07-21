using SmartTaskManagement.Domain.Entities;

namespace SmartTaskManagement.Domain.Enums;

/// <summary>
/// Lifecycle state of a <see cref="TaskItem"/>. Named TaskItemStatus rather than
/// TaskStatus to avoid colliding with <see cref="TaskStatus"/>.
/// </summary>
public enum TaskItemStatus
{
    ToDo,
    InProgress,
    Completed,
    Cancelled
}
