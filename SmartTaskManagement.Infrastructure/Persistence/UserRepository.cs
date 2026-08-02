using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using SmartTaskManagement.Application.Abstractions;
using SmartTaskManagement.Application.Authorization;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.Users.Dtos;
using SmartTaskManagement.Infrastructure.Identity;

namespace SmartTaskManagement.Infrastructure.Persistence;

/// <summary>
/// EF Core read repository for user lookup and management queries.
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<UserLookupDto>> QueryAssigneesAsync(
        AssigneeQueryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var query = QueryUsers(request.Search, searchEmail: false)
            .Join(
                _dbContext.UserRoles,
                user => user.Id,
                userRole => userRole.UserId,
                (user, userRole) => new { user, userRole })
            .Join(
                _dbContext.Roles,
                item => item.userRole.RoleId,
                role => role.Id,
                (item, role) => new { item.user, role })
            .Where(item => item.role.Name == RoleNames.TeamMember)
            .Select(item => item.user)
            .Distinct();

        query = query
            .OrderBy(user => user.FullName ?? string.Empty)
            .ThenBy(user => user.Email ?? string.Empty)
            .ThenBy(user => user.Id);

        return await ToPagedResultAsync(
            query,
            user => new UserLookupDto(user.Id, user.FullName ?? string.Empty, user.Email ?? string.Empty),
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }

    public async Task<PagedResult<UserListItemDto>> QueryUsersAsync(
        UserQueryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var query = QueryUsers(request.Search, searchEmail: true)
            .GroupJoin(
                _dbContext.UserRoles,
                user => user.Id,
                userRole => userRole.UserId,
                (user, userRoles) => new { user, userRoles })
            .SelectMany(
                item => item.userRoles.DefaultIfEmpty(),
                (item, userRole) => new { item.user, userRole })
            .GroupJoin(
                _dbContext.Roles,
                item => item.userRole!.RoleId,
                role => role.Id,
                (item, roles) => new { item.user, roles })
            .SelectMany(
                item => item.roles.DefaultIfEmpty(),
                (item, role) => new { item.user, role });

        if (!string.IsNullOrWhiteSpace(request.Role))
            query = query.Where(item => item.role != null && item.role.Name == request.Role);

        var orderedQuery = request.SortField switch
        {
            UserSortField.Name => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(item => item.user.FullName ?? string.Empty).ThenBy(item => item.user.Id)
                : query.OrderByDescending(item => item.user.FullName ?? string.Empty).ThenByDescending(item => item.user.Id),
            UserSortField.Role => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(item => item.role == null ? string.Empty : item.role.Name ?? string.Empty).ThenBy(item => item.user.Email).ThenBy(item => item.user.Id)
                : query.OrderByDescending(item => item.role == null ? string.Empty : item.role.Name ?? string.Empty).ThenByDescending(item => item.user.Email).ThenByDescending(item => item.user.Id),
            _ => request.SortDirection == SortDirection.Asc
                ? query.OrderBy(item => item.user.Email).ThenBy(item => item.user.Id)
                : query.OrderByDescending(item => item.user.Email).ThenByDescending(item => item.user.Id)
        };

        return await ToPagedResultAsync(
            orderedQuery,
            item => new UserListItemDto(
                item.user.Id,
                item.user.Email ?? string.Empty,
                item.user.FullName,
                item.role != null ? item.role.Name ?? string.Empty : string.Empty),
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }

    private IQueryable<ApplicationUser> QueryUsers(string? searchTerm, bool searchEmail)
    {
        var query = _dbContext.Users.AsNoTracking();
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var pattern = $"{searchTerm.Trim()}%";
        return query.Where(user =>
            (user.FullName != null &&
             EF.Functions.Like(user.FullName, pattern)) ||
            (searchEmail && user.Email != null &&
             EF.Functions.Like(user.Email, pattern)));
    }

    private static async Task<PagedResult<TResult>> ToPagedResultAsync<TSource, TResult>(
        IQueryable<TSource> query,
        Expression<Func<TSource, TResult>> selector,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(selector)
            .ToListAsync(cancellationToken);

        return new PagedResult<TResult>(items, totalCount, pageNumber, pageSize);
    }

}
