using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SmartTaskManagement.Application.Abstractions;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.Tasks.Dtos;
using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Domain.Enums;

namespace SmartTaskManagement.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of <see cref="ITaskRepository"/>. Persistence details stay in
/// Infrastructure; the Application layer sees only the abstraction.
/// </summary>
public sealed class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TaskRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Tracked: callers (update/assign/status/delete) mutate the returned entity and save it.
    public Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TaskItem>> ListByProjectAsync(Guid projectId, Guid? assignedToUserId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Tasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId);

        // Scope to a single assignee database-side when requested (Team Member's own tasks).
        if (assignedToUserId is { } userId)
            query = query.Where(t => t.AssignedToUserId == userId);

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<TaskItem>> QueryAsync(
        TaskQueryRequestDto request,
        Guid? assignedToUserId,
        Guid? projectOwnerUserId,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyVisibility(assignedToUserId, projectOwnerUserId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(t =>
                EF.Functions.Like(t.Title, $"%{search}%") ||
                EF.Functions.Like(t.Description!, $"%{search}%"));
        }

        if (request.Status.HasValue)
            query = query.Where(t => t.Status == request.Status.Value);

        if (request.Priority.HasValue)
            query = query.Where(t => t.Priority == request.Priority.Value);

        if (request.DueDate.HasValue)
            query = query.Where(t => t.DueDate <= request.DueDate.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.SortField switch
        {
            TaskSortField.Title => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(t => t.Title)
                : query.OrderByDescending(t => t.Title),
            TaskSortField.DueDate => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(t => t.DueDate)
                : query.OrderByDescending(t => t.DueDate),
            TaskSortField.Priority => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(t => t.Priority)
                : query.OrderByDescending(t => t.Priority),
            TaskSortField.Status => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(t => t.Status)
                : query.OrderByDescending(t => t.Status),
            _ => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(t => t.CreatedAt)
                : query.OrderByDescending(t => t.CreatedAt)
        };

        query = query.Skip((request.PageNumber - 1) * request.PageSize)
                     .Take(request.PageSize);

        var items = await query.ToListAsync(cancellationToken);
        return new PagedResult<TaskItem>(items, totalCount, request.PageNumber, request.PageSize);
    }

    private IQueryable<TaskItem> ApplyVisibility(Guid? assignedToUserId, Guid? projectOwnerUserId)
    {
        var query = _dbContext.Tasks.AsNoTracking().AsQueryable();

        if (assignedToUserId.HasValue)
        {
            var userId = assignedToUserId.Value;
            query = query.Where(t => t.AssignedToUserId == userId);
        }

        if (projectOwnerUserId.HasValue)
        {
            var ownerId = projectOwnerUserId.Value;
            query = query.Where(t => _dbContext.Projects.Any(p => p.Id == t.ProjectId && p.CreatedByUserId == ownerId));
        }

        return query;
    }

    public async Task<int> CountAsync(
        Expression<Func<TaskItem, bool>> predicate,
        Guid? assignedToUserId,
        Guid? projectOwnerUserId,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyVisibility(assignedToUserId, projectOwnerUserId)
            .Where(predicate);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<Dictionary<TaskItemStatus, int>> CountByStatusAsync(
        Guid? assignedToUserId,
        Guid? projectOwnerUserId,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyVisibility(assignedToUserId, projectOwnerUserId);

        var counts = await query
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return Enum.GetValues<TaskItemStatus>()
            .ToDictionary(status => status, status => counts.FirstOrDefault(c => c.Status == status)?.Count ?? 0);
    }

    public async Task<Dictionary<TaskItemPriority, int>> CountByPriorityAsync(
        Guid? assignedToUserId,
        Guid? projectOwnerUserId,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyVisibility(assignedToUserId, projectOwnerUserId);

        var counts = await query
            .GroupBy(t => t.Priority)
            .Select(g => new { Priority = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return Enum.GetValues<TaskItemPriority>()
            .ToDictionary(priority => priority, priority => counts.FirstOrDefault(c => c.Priority == priority)?.Count ?? 0);
    }

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        _dbContext.Tasks.Update(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        _dbContext.Tasks.Remove(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
