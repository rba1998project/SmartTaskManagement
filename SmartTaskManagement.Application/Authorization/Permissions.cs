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
    public const string TasksStatusUpdate = "tasks.status.update";
    public const string TasksStatusHistoryView = "tasks.status-history.view";
    public const string UsersManage = "users.manage";

    // All permission keys used to register authorization policies.
    public static readonly IReadOnlyList<string> AllPermissions = new[]
    {
        ProjectsCreate, ProjectsUpdate, ProjectsDelete,
        TasksCreate, TasksUpdate, TasksDelete, TasksAssign,
        TasksStatusUpdate, TasksStatusHistoryView, UsersManage
    };

    // Permissions granted to Admin through role claims. Team-Member-only status updates are
    // registered as a policy above but deliberately excluded from Admin claims.
    public static readonly IReadOnlyList<string> AdminPermissions = new[]
    {
        ProjectsCreate, ProjectsUpdate, ProjectsDelete,
        TasksCreate, TasksUpdate, TasksDelete, TasksAssign,
        TasksStatusHistoryView, UsersManage
    };

    /// <summary>
    /// Default permissions granted to each role at seeding. Admin gets every general permission;
    /// Project Manager gets project/task permissions plus status-history viewing. Team Member gets
    /// the status-change permission; service rules still restrict the workflow to assigned tasks.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> DefaultRolePermissions =
        new Dictionary<string, IReadOnlyList<string>>
        {
            // Admin receives all general permissions; the status-update policy remains
            // Team-Member-only through the role claim and service-level workflow checks.
            [RoleNames.Admin] = AdminPermissions,
            [RoleNames.ProjectManager] = new[]
            {
                ProjectsCreate, ProjectsUpdate, ProjectsDelete,
                TasksCreate, TasksUpdate, TasksDelete, TasksAssign,
                TasksStatusHistoryView
            },
            [RoleNames.TeamMember] = new[] { TasksStatusUpdate }
        };
}
