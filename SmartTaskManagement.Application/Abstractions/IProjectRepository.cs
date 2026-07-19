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

    /// <summary>Adds and persists a new project.</summary>
    Task AddAsync(Project project, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing tracked project.</summary>
    Task UpdateAsync(Project project, CancellationToken cancellationToken = default);

    /// <summary>Removes and persists deletion of the project.</summary>
    Task RemoveAsync(Project project, CancellationToken cancellationToken = default);
}
