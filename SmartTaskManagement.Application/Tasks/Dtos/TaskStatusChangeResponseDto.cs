using SmartTaskManagement.Domain.Enums;

namespace SmartTaskManagement.Application.Tasks.Dtos;

/// <summary>Read-only task status history entry.</summary>
public sealed class TaskStatusChangeResponseDto
{
    public Guid Id { get; init; }
    public TaskItemStatus FromStatus { get; init; }
    public TaskItemStatus ToStatus { get; init; }
    public string? Comment { get; init; }
    public Guid ChangedByUserId { get; init; }
    public string ChangedByDisplayName { get; init; } = string.Empty;
    public DateTime ChangedAt { get; init; }
}
