using SmartTaskManagement.Domain.Entities;

namespace SmartTaskManagement.Application.Abstractions;

/// <summary>
/// Persistence for <see cref="Project"/> aggregates. A specific repository (not a generic one).
/// Implemented in Infrastructure with EF Core. Mutating methods persist their change.
/// </summary>
public interface IProjectRepository
{
    /// <summary>Returns the project by id, or <c>null</c> if none exists.</summary>
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Returns all projects (read-only).</summary>
    Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the projects that contain at least one task assigned to
    /// <paramref name="assignedToUserId"/> (read-only). Filtering is done database-side so a
    /// Team Member's project list is scoped to their assignments without an N+1 fan-out.
    /// </summary>
    Task<IReadOnlyList<Project>> ListByAssignedUserAsync(Guid assignedToUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns <c>true</c> when the given project contains at least one task assigned to
    /// <paramref name="assignedToUserId"/>. Used to gate a Team Member's access to a single
    /// project's details database-side.
    /// </summary>
    Task<bool> HasTaskAssignedToUserAsync(Guid projectId, Guid assignedToUserId, CancellationToken cancellationToken = default);

    /// <summary>Adds and persists a new project.</summary>
    Task AddAsync(Project project, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing tracked project.</summary>
    Task UpdateAsync(Project project, CancellationToken cancellationToken = default);

    /// <summary>Removes and persists deletion of the project.</summary>
    Task RemoveAsync(Project project, CancellationToken cancellationToken = default);
}
