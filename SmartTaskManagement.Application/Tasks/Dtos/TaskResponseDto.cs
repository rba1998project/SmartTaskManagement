using SmartTaskManagement.Domain.Enums;

namespace SmartTaskManagement.Application.Tasks.Dtos;

/// <summary>
/// Task representation returned at the API boundary — keeps the Domain entity from leaking.
/// </summary>
public sealed class TaskResponseDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public TaskItemStatus Status { get; init; }
    public TaskItemPriority Priority { get; init; }
    public DateTime? DueDate { get; init; }
    public Guid? AssignedToUserId { get; init; }
    public string? AssignedToUserName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
