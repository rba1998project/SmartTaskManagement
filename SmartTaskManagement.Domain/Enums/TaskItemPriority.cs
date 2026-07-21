using SmartTaskManagement.Domain.Entities;

namespace SmartTaskManagement.Domain.Enums;

/// <summary>
/// Priority of a <see cref="TaskItem"/>.
/// </summary>
public enum TaskItemPriority
{
    Low,
    Medium,
    High,
    Critical
}
