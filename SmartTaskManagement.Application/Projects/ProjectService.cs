using SmartTaskManagement.Application.Abstractions;
using SmartTaskManagement.Application.Authorization;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.Projects.Dtos;
using SmartTaskManagement.Domain.Entities;

namespace SmartTaskManagement.Application.Projects;

/// <summary>
/// Project use cases: create, update, delete, details, list. Controllers stay thin by
/// delegating here. Resource-ownership rules live in this layer (not the API): an Admin may
/// modify any project; a Project Manager only projects they own. All authenticated users may
/// view and list. Role-gating of who may reach create/update/delete at all is enforced at the
/// API with <c>[Authorize(Policy = ...)]</c>; the ownership check here is the second gate.
/// Expected failures are returned as a categorized <see cref="Result"/> — never exceptions.
/// </summary>
public sealed class ProjectService
{
    private readonly IProjectRepository _projects;
    private readonly ICurrentUserService _currentUser;
    private readonly IIdentityService _identity;

    public ProjectService(IProjectRepository projects, ICurrentUserService currentUser, IIdentityService identity)
    {
        _projects = projects;
        _currentUser = currentUser;
        _identity = identity;
    }

    public async Task<Result<ProjectResponseDto>> CreateAsync(CreateProjectRequestDto request, CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is not { } userId)
            return Result<ProjectResponseDto>.Failure(ErrorType.Forbidden, "Not authenticated.");

        var project = new Project(request.Name, request.Description, userId, DateTime.UtcNow);
        await _projects.AddAsync(project, cancellationToken);

        return Result<ProjectResponseDto>.Success(Map(project));
    }

    public async Task<Result<ProjectResponseDto>> UpdateAsync(Guid id, UpdateProjectRequestDto request, CancellationToken cancellationToken = default)
    {
        var project = await _projects.GetByIdAsync(id, cancellationToken);
        if (project is null)
            return Result<ProjectResponseDto>.Failure(ErrorType.NotFound, "Project not found.");

        if (!CanModify(project))
            return Result<ProjectResponseDto>.Failure(ErrorType.Forbidden, "You do not have permission to modify this project.");

        project.Update(request.Name, request.Description, DateTime.UtcNow);
        await _projects.UpdateAsync(project, cancellationToken);

        return Result<ProjectResponseDto>.Success(Map(project));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _projects.GetByIdAsync(id, cancellationToken);
        if (project is null)
            return Result.Failure(ErrorType.NotFound, "Project not found.");

        if (!CanModify(project))
            return Result.Failure(ErrorType.Forbidden, "You do not have permission to delete this project.");

        await _projects.RemoveAsync(project, cancellationToken);
        return Result.Success();
    }

    public async Task<Result<ProjectResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _projects.GetByIdAsync(id, cancellationToken);
        if (project is null)
            return Result<ProjectResponseDto>.Failure(ErrorType.NotFound, "Project not found.");

        // A Team Member may only view a project that contains a task assigned to them. Admin and
        // the owning Project Manager may view it regardless of assignment.
        if (!CanView() && !await IsVisibleToTeamMemberAsync(project.Id, cancellationToken))
            return Result<ProjectResponseDto>.Failure(ErrorType.NotFound, "Project not found.");

        var creator = await _identity.FindByIdAsync(project.CreatedByUserId, cancellationToken);
        return Result<ProjectResponseDto>.Success(Map(project, creator?.FullName));
    }

    public async Task<Result<PagedResult<ProjectResponseDto>>> ListAsync(ProjectQueryRequestDto request, CancellationToken cancellationToken = default)
    {
        // A Team Member sees only projects containing tasks assigned to them (filtered
        // database-side); Admin and Project Managers see all projects.
        Guid? teamMemberUserId = null;

        if (_currentUser.IsInRole(RoleNames.TeamMember) && !_currentUser.IsInRole(RoleNames.Admin))
            teamMemberUserId = _currentUser.UserId;

        var pagedResult = await _projects.QueryAsync(request, teamMemberUserId, cancellationToken);

        var creatorIds = pagedResult.Items
            .Select(p => p.CreatedByUserId)
            .Distinct()
            .ToArray();

        var creators = await _identity.FindByIdsAsync(creatorIds, cancellationToken);
        var creatorMap = creators.ToDictionary(u => u.Id, u => u.FullName);

        var mapped = new PagedResult<ProjectResponseDto>(
            pagedResult.Items.Select(p => Map(p, creatorMap.TryGetValue(p.CreatedByUserId, out var name) ? name : null)).ToArray(),
            pagedResult.TotalCount,
            pagedResult.PageNumber,
            pagedResult.PageSize);

        return Result<PagedResult<ProjectResponseDto>>.Success(mapped);
    }

    // Admin may modify any project; anyone else only projects they own. The API role-gate
    // already excludes Team Members from these operations, so this covers Admin vs owner.
    private bool CanModify(Project project)
    {
        if (_currentUser.IsInRole(RoleNames.Admin))
            return true;

        return _currentUser.IsInRole(RoleNames.ProjectManager)
            && project.CreatedByUserId == _currentUser.UserId;
    }

    // Admin and Project Managers retain unrestricted project visibility. Only a Team Member is
    // narrowed to projects containing tasks assigned to them (checked separately, database-side).
    private bool CanView() => _currentUser.IsInRole(RoleNames.Admin) || !_currentUser.IsInRole(RoleNames.TeamMember);

    // True when the current Team Member has a task assigned within the project.
    private async Task<bool> IsVisibleToTeamMemberAsync(Guid projectId, CancellationToken cancellationToken) =>
        _currentUser.UserId is { } userId
        && await _projects.HasTaskAssignedToUserAsync(projectId, userId, cancellationToken);

    private static ProjectResponseDto Map(Project project, string? createdByUserName = null) => new()
    {
        Id = project.Id,
        Name = project.Name,
        Description = project.Description,
        CreatedByUserId = project.CreatedByUserId,
        CreatedByUserName = createdByUserName,
        CreatedAt = project.CreatedAt,
        UpdatedAt = project.UpdatedAt,
    };
}
