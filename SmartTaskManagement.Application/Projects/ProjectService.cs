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
/// API with <c>[Authorize(Roles = ...)]</c>; the ownership check here is the second gate.
/// Expected failures are returned as a categorized <see cref="Result"/> — never exceptions.
/// </summary>
public sealed class ProjectService
{
    private readonly IProjectRepository _projects;
    private readonly ICurrentUserService _currentUser;

    public ProjectService(IProjectRepository projects, ICurrentUserService currentUser)
    {
        _projects = projects;
        _currentUser = currentUser;
    }

    public async Task<Result<ProjectResponse>> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is not { } userId)
            return Result<ProjectResponse>.Failure(ErrorType.Forbidden, "Not authenticated.");

        var project = new Project(request.Name, request.Description, userId, DateTime.UtcNow);
        await _projects.AddAsync(project, cancellationToken);

        return Result<ProjectResponse>.Success(Map(project));
    }

    public async Task<Result<ProjectResponse>> UpdateAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var project = await _projects.GetByIdAsync(id, cancellationToken);
        if (project is null)
            return Result<ProjectResponse>.Failure(ErrorType.NotFound, "Project not found.");

        if (!CanModify(project))
            return Result<ProjectResponse>.Failure(ErrorType.Forbidden, "You do not have permission to modify this project.");

        project.Update(request.Name, request.Description, DateTime.UtcNow);
        await _projects.UpdateAsync(project, cancellationToken);

        return Result<ProjectResponse>.Success(Map(project));
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

    public async Task<Result<ProjectResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _projects.GetByIdAsync(id, cancellationToken);
        return project is null
            ? Result<ProjectResponse>.Failure(ErrorType.NotFound, "Project not found.")
            : Result<ProjectResponse>.Success(Map(project));
    }

    public async Task<Result<IReadOnlyList<ProjectResponse>>> ListAsync(CancellationToken cancellationToken = default)
    {
        var projects = await _projects.ListAsync(cancellationToken);
        IReadOnlyList<ProjectResponse> mapped = projects.Select(Map).ToArray();
        return Result<IReadOnlyList<ProjectResponse>>.Success(mapped);
    }

    // Admin may modify any project; anyone else only projects they own. The API role-gate
    // already excludes Team Members from these operations, so this covers Admin vs owner.
    private bool CanModify(Project project) =>
        _currentUser.IsInRole(RoleNames.Admin) || project.CreatedByUserId == _currentUser.UserId;

    private static ProjectResponse Map(Project project) => new()
    {
        Id = project.Id,
        Name = project.Name,
        Description = project.Description,
        CreatedByUserId = project.CreatedByUserId,
        CreatedAt = project.CreatedAt,
        UpdatedAt = project.UpdatedAt,
    };
}
