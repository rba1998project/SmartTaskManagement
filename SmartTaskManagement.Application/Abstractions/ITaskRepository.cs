using System.Linq.Expressions;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.Tasks.Dtos;
using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Domain.Enums;

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

    /// <summary>
    /// Returns tasks matching the query. Visibility is controlled by the two optional
    /// parameters: pass <paramref name="assignedToUserId"/> to scope to a Team Member's
    /// assignments, or <paramref name="projectOwnerUserId"/> to scope to tasks in projects
    /// owned by a Project Manager. Pass <c>null</c> for both to return all tasks (Admin).
    /// </summary>
    Task<PagedResult<TaskItem>> QueryAsync(
        TaskQueryRequestDto request,
        Guid? assignedToUserId,
        Guid? projectOwnerUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the count of tasks matching <paramref name="predicate"/>, scoped to the
    /// current user's visibility rules. Pass <c>null</c> for both visibility parameters
    /// to return all tasks (Admin).
    /// </summary>
    Task<int> CountAsync(
        Expression<Func<TaskItem, bool>> predicate,
        Guid? assignedToUserId,
        Guid? projectOwnerUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the count of visible tasks grouped by <see cref="TaskItemStatus"/>.
    /// The returned dictionary contains every enum value; missing values have count zero.
    /// </summary>
    Task<Dictionary<TaskItemStatus, int>> CountByStatusAsync(
        Guid? assignedToUserId,
        Guid? projectOwnerUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the count of visible tasks grouped by <see cref="TaskItemPriority"/>.
    /// The returned dictionary contains every enum value; missing values have count zero.
    /// </summary>
    Task<Dictionary<TaskItemPriority, int>> CountByPriorityAsync(
        Guid? assignedToUserId,
        Guid? projectOwnerUserId,
        CancellationToken cancellationToken = default);

    /// <summary>Adds and persists a new task.</summary>
    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing tracked task.</summary>
    Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default);
}
