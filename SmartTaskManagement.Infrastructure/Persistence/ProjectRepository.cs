using Microsoft.EntityFrameworkCore;
using SmartTaskManagement.Application.Abstractions;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.Projects.Dtos;
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

    public async Task<IReadOnlyList<Project>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var distinctIds = ids.Distinct().ToArray();
        if (distinctIds.Length == 0)
            return Array.Empty<Project>();

        return await _dbContext.Projects
            .Where(p => distinctIds.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<bool> HasTaskAssignedToUserAsync(Guid projectId, Guid assignedToUserId, CancellationToken cancellationToken = default) =>
        _dbContext.Tasks.AnyAsync(t => t.ProjectId == projectId && t.AssignedToUserId == assignedToUserId, cancellationToken);

    public async Task<PagedResult<Project>> QueryAsync(
        ProjectQueryRequestDto request,
        Guid? teamMemberUserId,
        Guid? projectOwnerUserId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Projects.AsNoTracking().AsQueryable();

        if (teamMemberUserId.HasValue)
        {
            var userId = teamMemberUserId.Value;
            query = query.Where(p => _dbContext.Tasks.Any(t => t.ProjectId == p.Id && t.AssignedToUserId == userId));
        }

        if (projectOwnerUserId.HasValue)
        {
            var ownerId = projectOwnerUserId.Value;
            query = query.Where(p => p.CreatedByUserId == ownerId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(p =>
                EF.Functions.Like(p.Name, $"%{search}%") ||
                EF.Functions.Like(p.Description!, $"%{search}%"));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.SortField switch
        {
            ProjectSortFieldDto.Name => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(p => p.Name)
                : query.OrderByDescending(p => p.Name),
            ProjectSortFieldDto.UpdatedAt => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(p => p.UpdatedAt)
                : query.OrderByDescending(p => p.UpdatedAt),
            _ => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt)
        };

        query = query.Skip((request.PageNumber - 1) * request.PageSize)
                     .Take(request.PageSize);

        var items = await query.ToListAsync(cancellationToken);
        return new PagedResult<Project>(items, totalCount, request.PageNumber, request.PageSize);
    }

    public async Task<int> CountVisibleAsync(Guid? teamMemberUserId, Guid? projectOwnerUserId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Projects.AsNoTracking().AsQueryable();

        if (teamMemberUserId.HasValue)
        {
            var userId = teamMemberUserId.Value;
            query = query.Where(p => _dbContext.Tasks.Any(t => t.ProjectId == p.Id && t.AssignedToUserId == userId));
        }

        if (projectOwnerUserId.HasValue)
        {
            var ownerId = projectOwnerUserId.Value;
            query = query.Where(p => p.CreatedByUserId == ownerId);
        }

        return await query.CountAsync(cancellationToken);
    }

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

    public Task RemoveAsync(Project project, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Hard delete is no longer supported.");
    }

    public async Task PersistSoftDeleteAsync(Project project, IEnumerable<TaskItem> tasks, CancellationToken cancellationToken = default)
    {
        _dbContext.Tasks.UpdateRange(tasks);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
