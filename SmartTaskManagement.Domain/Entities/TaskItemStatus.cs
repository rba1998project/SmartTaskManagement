namespace SmartTaskManagement.Domain.Entities;

/// <summary>
/// Lifecycle state of a <see cref="TaskItem"/>. Named TaskItemStatus rather than
/// TaskStatus to avoid colliding with <see cref="System.Threading.Tasks.TaskStatus"/>.
/// </summary>
public enum TaskItemStatus
{
    ToDo,
    InProgress,
    Completed,
    Cancelled
}
