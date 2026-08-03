using SmartTaskManagement.Domain.Enums;

namespace SmartTaskManagement.Domain.Entities;

/// <summary>
/// Immutable audit record for a task status transition. The actor display name is
/// snapshotted so historical records remain stable if the Identity user changes later.
/// </summary>
public sealed class TaskStatusChange
{
    private const int CommentMaxLength = 1000;
    private const int ChangedByDisplayNameMaxLength = 256;

    public Guid Id { get; private set; }
    public Guid TaskId { get; private set; }
    public TaskItemStatus FromStatus { get; private set; }
    public TaskItemStatus ToStatus { get; private set; }
    public string? Comment { get; private set; }
    public Guid ChangedByUserId { get; private set; }
    public string ChangedByDisplayName { get; private set; } = string.Empty;
    public DateTime ChangedAt { get; private set; }

    private TaskStatusChange() { }

    public TaskStatusChange(
        Guid taskId,
        TaskItemStatus fromStatus,
        TaskItemStatus toStatus,
        string? comment,
        Guid changedByUserId,
        string changedByDisplayName,
        DateTime changedAt)
    {
        if (taskId == Guid.Empty)
            throw new ArgumentException("Task id is required.", nameof(taskId));
        if (fromStatus == toStatus)
            throw new ArgumentException("A status change must change the status.", nameof(toStatus));
        if (changedByUserId == Guid.Empty)
            throw new ArgumentException("Changed-by user id is required.", nameof(changedByUserId));
        if (string.IsNullOrWhiteSpace(changedByDisplayName))
            throw new ArgumentException("Changed-by display name is required.", nameof(changedByDisplayName));

        var normalizedComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        if (normalizedComment is not null && normalizedComment.Length > CommentMaxLength)
            throw new ArgumentException($"Comment must be at most {CommentMaxLength} characters.", nameof(comment));

        Id = Guid.NewGuid();
        TaskId = taskId;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        Comment = normalizedComment;
        ChangedByUserId = changedByUserId;
        ChangedByDisplayName = changedByDisplayName.Trim()[..Math.Min(changedByDisplayName.Trim().Length, ChangedByDisplayNameMaxLength)];
        ChangedAt = changedAt;
    }
}
