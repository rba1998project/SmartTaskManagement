using Microsoft.EntityFrameworkCore;
using SmartTaskManagement.Application.Abstractions;
using SmartTaskManagement.Application.Authorization;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.Users.Dtos;

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
        var query =
            from user in _dbContext.Users.AsNoTracking()
            join userRole in _dbContext.UserRoles on user.Id equals userRole.UserId
            join role in _dbContext.Roles on userRole.RoleId equals role.Id
            where role.Name == RoleNames.TeamMember
            select user;

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(u =>
                u.FullName != null && EF.Functions.Like(u.FullName, $"{search}%"));
        }

        query = query
            .Distinct()
            .OrderBy(u => u.FullName ?? string.Empty)
            .ThenBy(u => u.Email ?? string.Empty)
            .ThenBy(u => u.Id);

        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserLookupDto(u.Id, u.FullName ?? string.Empty, u.Email ?? string.Empty))
            .ToListAsync(cancellationToken);

        return new PagedResult<UserLookupDto>(users, totalCount, request.PageNumber, request.PageSize);
    }

    public async Task<IReadOnlyList<UserManagementDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await (
            from user in _dbContext.Users.AsNoTracking()
            join userRole in _dbContext.UserRoles on user.Id equals userRole.UserId into userRoles
            from userRole in userRoles.DefaultIfEmpty()
            join role in _dbContext.Roles on userRole.RoleId equals role.Id into roles
            from role in roles.DefaultIfEmpty()
            orderby user.Email, user.Id
            select new UserManagementDto(
                user.Id,
                user.Email ?? string.Empty,
                user.FullName,
                role != null ? role.Name ?? string.Empty : string.Empty))
            .ToListAsync(cancellationToken);

        return users;
    }
}
