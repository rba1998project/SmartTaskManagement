using Microsoft.EntityFrameworkCore;
using SmartTaskManagement.Application.Abstractions;
using SmartTaskManagement.Domain.Entities;

namespace SmartTaskManagement.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="IProjectRepository"/>. Persistence details stay in
/// Infrastructure; the Application layer sees only the abstraction. 
/// </summary>
public sealed class ProjectRepository : IProjectRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ProjectRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Tracked: callers (update/delete) mutate the returned entity and save it.
    public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Projects.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Projects
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    // Database-side semi-join: keep only projects that have a task assigned to this user. The
    // subquery avoids fetching tasks and prevents an N+1 fan-out over the project list.
    public async Task<IReadOnlyList<Project>> ListByAssignedUserAsync(Guid assignedToUserId, CancellationToken cancellationToken = default) =>
        await _dbContext.Projects
            .AsNoTracking()
            .Where(p => _dbContext.Tasks.Any(t => t.ProjectId == p.Id && t.AssignedToUserId == assignedToUserId))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<bool> HasTaskAssignedToUserAsync(Guid projectId, Guid assignedToUserId, CancellationToken cancellationToken = default) =>
        _dbContext.Tasks.AnyAsync(t => t.ProjectId == projectId && t.AssignedToUserId == assignedToUserId, cancellationToken);

    public async Task AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        _dbContext.Projects.Update(project);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Project project, CancellationToken cancellationToken = default)
    {
        _dbContext.Projects.Remove(project);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
