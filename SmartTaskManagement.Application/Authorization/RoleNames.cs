namespace SmartTaskManagement.Application.Authorization;

/// <summary>
/// Canonical role names — the single source of truth for role identifiers used in
/// role seeding and <c>[Authorize(Roles = ...)]</c> checks. No parallel enum exists:
/// roles carry no domain behavior, so a second representation would only be a sync burden.
/// </summary>
public static class RoleNames
{
    public const string Admin = "Admin";
    public const string ProjectManager = "ProjectManager";
    public const string TeamMember = "TeamMember";

    //All role names, for convenient seeding.
    public static readonly IReadOnlyList<string> All = new[] { Admin, ProjectManager, TeamMember };
}
