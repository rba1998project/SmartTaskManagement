using SmartTaskManagement.Domain.Entities;

namespace SmartTaskManagement.Application.Abstractions;

/// <summary>
/// Persistence for <see cref="TaskItem"/> aggregates. A specific repository (not a generic one).
/// Implemented in Infrastructure with EF Core. Mutating methods persist their change.
/// </summary>
public interface ITaskRepository
{
    /// <summary>Returns the task by id, or <c>null</c> if none exists.</summary>
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the tasks in a project (read-only). When <paramref name="assignedToUserId"/> is
    /// supplied the results are filtered database-side to that assignee — used to give a Team
    /// Member only the tasks assigned to them.
    /// </summary>
    Task<IReadOnlyList<TaskItem>> ListByProjectAsync(Guid projectId, Guid? assignedToUserId = null, CancellationToken cancellationToken = default);

    /// <summary>Adds and persists a new task.</summary>
    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing tracked task.</summary>
    Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default);

    /// <summary>Removes and persists deletion of the task.</summary>
    Task RemoveAsync(TaskItem task, CancellationToken cancellationToken = default);
}
