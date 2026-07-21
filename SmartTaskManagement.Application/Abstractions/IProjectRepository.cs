using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.Projects.Dtos;
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

    /// <summary>
    /// Returns <c>true</c> when the given project contains at least one task assigned to
    /// <paramref name="assignedToUserId"/>. Used to gate a Team Member's access to a single
    /// project's details database-side.
    /// </summary>
    Task<bool> HasTaskAssignedToUserAsync(Guid projectId, Guid assignedToUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns projects matching the query, scoped to the current user's visibility rules.
    /// When <paramref name="teamMemberUserId"/> is provided, only projects containing a task
    /// assigned to that user are returned (Team Member visibility). Pass <c>null</c> for
    /// Admin / Project Manager to return all projects.
    /// </summary>
    Task<PagedResult<Project>> QueryAsync(
        ProjectQueryRequestDto request,
        Guid? teamMemberUserId,
        CancellationToken cancellationToken = default);

    /// <summary>Adds and persists a new project.</summary>
    Task AddAsync(Project project, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing tracked project.</summary>
    Task UpdateAsync(Project project, CancellationToken cancellationToken = default);

    /// <summary>Removes and persists deletion of the project.</summary>
    Task RemoveAsync(Project project, CancellationToken cancellationToken = default);
}
