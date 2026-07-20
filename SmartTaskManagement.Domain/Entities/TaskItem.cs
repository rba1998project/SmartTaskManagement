namespace SmartTaskManagement.Domain.Entities;

/// <summary>
/// Named TaskItem rather
/// than Task to avoid colliding with <see cref="System.Threading.Tasks.Task"/>. The owning project is referenced solely by
/// <see cref="ProjectId"/> and the optional assignee solely by <see cref="AssignedToUserId"/>
/// (the Identity user id) — there are no navigations to the Identity type, keeping the Domain
/// free of Infrastructure/Identity dependencies. Creation and update rules live here so the
/// entity is a genuine domain type rather than a bag of properties. The current time is
/// passed in by callers so the Domain stays free of DateTime.UtcNow, which helps to test.
/// </summary>
public class TaskItem
{
    public Guid Id { get; private set; }

    // Owning project. A task always belongs to exactly one project, so task to project relationship is many-to-one.
    public Guid ProjectId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public TaskItemStatus Status { get; private set; }

    public TaskItemPriority Priority { get; private set; }

    public DateTime? DueDate { get; private set; }

    // Optional assignee's Identity user id. No navigation to the Identity type keeps the Domain
    // pure; null means the task is unassigned. Single-user assignment model.
    public Guid? AssignedToUserId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    // EF Core materialization constructor.
    private TaskItem() { }

    public TaskItem(
        Guid projectId,
        string title,
        string? description,
        TaskItemPriority priority,
        DateTime? dueDate,
        DateTime createdAt)
    {
        if (projectId == Guid.Empty)
            throw new ArgumentException("Project id is required.", nameof(projectId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title is required.", nameof(title));

        Id = Guid.NewGuid();
        ProjectId = projectId;
        Title = title.Trim();
        Description = NormalizeDescription(description);
        Status = TaskItemStatus.ToDo;
        Priority = priority;
        DueDate = dueDate;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    /// <summary>
    /// Applies an update to the editable task details and stamps <see cref="UpdatedAt"/>.
    /// The owning project never changes.
    /// </summary>
    public void UpdateDetails(
        string title,
        string? description,
        TaskItemPriority priority,
        DateTime? dueDate,
        DateTime updatedAt)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title is required.", nameof(title));

        Title = title.Trim();
        Description = NormalizeDescription(description);
        Priority = priority;
        DueDate = dueDate;
        UpdatedAt = updatedAt;
    }

    /// <summary>
    /// Changes the task status and stamps <see cref="UpdatedAt"/>. This is the only mutation a
    /// Team Member is permitted to make on their assigned tasks.
    /// </summary>
    public void ChangeStatus(TaskItemStatus status, DateTime updatedAt)
    {
        Status = status;
        UpdatedAt = updatedAt;
    }

    /// <summary>
    /// Assigns the task to an application user (or clears the assignment when
    /// <paramref name="userId"/> is null) and stamps <see cref="UpdatedAt"/>.
    /// </summary>
    public void AssignTo(Guid? userId, DateTime updatedAt)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Assignee user id must be a valid user or null.", nameof(userId));

        AssignedToUserId = userId;
        UpdatedAt = updatedAt;
    }

    // Empty/whitespace descriptions collapse to null so "absent" has one representation.
    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();
}
