namespace SmartTaskManagement.Application.Authorization;

/// <summary>
/// Canonical permission keys and the default role-to-permission mapping. Permissions are the
/// feature gate: each protected endpoint requires one via [Authorize(Policy = ...)].
/// Stored as role claims (type <see cref="ClaimType"/>), seeded from <see cref="DefaultRolePermissions"/>,
/// and carried in the JWT so authorization needs no per-request database lookup.
/// </summary>
public static class Permissions
{
    /// <summary>Claim type used for permission claims on roles and in the JWT.</summary>
    public const string ClaimType = "permission";

    public const string ProjectsCreate = "projects.create";
    public const string ProjectsUpdate = "projects.update";
    public const string ProjectsDelete = "projects.delete";
    public const string TasksCreate = "tasks.create";
    public const string TasksUpdate = "tasks.update";
    public const string TasksDelete = "tasks.delete";
    public const string TasksAssign = "tasks.assign";
    public const string UsersManage = "users.manage";

    // All permission keys, for registering one authorization policy per permission.
    public static readonly IReadOnlyList<string> AllPermissions = new[]
    {
        ProjectsCreate, ProjectsUpdate, ProjectsDelete,
        TasksCreate, TasksUpdate, TasksDelete, TasksAssign,
        UsersManage
    };

    /// <summary>
    /// Default permissions granted to each role at seeding. Admin gets every permission.
    /// Project Manager gets project/task permissions only. Team Member gets none — it reaches only the open endpoints.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> DefaultRolePermissions =
        new Dictionary<string, IReadOnlyList<string>>
        {
            [RoleNames.Admin] = AllPermissions,
            [RoleNames.ProjectManager] = new[]
            {
                ProjectsCreate, ProjectsUpdate, ProjectsDelete,
                TasksCreate, TasksUpdate, TasksDelete, TasksAssign
            },
            [RoleNames.TeamMember] = Array.Empty<string>()
        };
}
