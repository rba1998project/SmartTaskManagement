using Microsoft.AspNetCore.Identity;

namespace SmartTaskManagement.Infrastructure.Identity;

/// <summary>
/// Identity role with a Guid key. Role names come from
/// <see cref="SmartTaskManagement.Application.Authorization.RoleNames"/>.
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }

    public ApplicationRole(string roleName) : base(roleName) { }
}
