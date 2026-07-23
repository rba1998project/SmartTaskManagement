using FluentValidation;
using SmartTaskManagement.Application.Users.Dtos;

namespace SmartTaskManagement.Application.Users.Validators;

/// <summary>
/// Validates role update input. The requested role must be one of the known application roles.
/// </summary>
public sealed class UpdateUserRoleRequestValidator : AbstractValidator<UpdateUserRoleRequest>
{
    private static readonly string[] AllowedRoles = { "Admin", "ProjectManager", "TeamMember" };

    public UpdateUserRoleRequestValidator()
    {
        RuleFor(x => x.RoleName)
            .Must(role => string.IsNullOrWhiteSpace(role) || AllowedRoles.Contains(role))
            .WithMessage("Role must be one of: Admin, ProjectManager, TeamMember.");
    }
}
