using FluentValidation;
using SmartTaskManagement.Application.Tasks.Dtos;

namespace SmartTaskManagement.Application.Tasks.Validators;

/// <summary>
/// Validates task assignment input. A null assignee is allowed (it clears the assignment);
/// a supplied assignee id must be a non-empty Guid. Whether the user actually exists is
/// checked in <see cref="TaskService"/> against the identity store.
/// </summary>
public sealed class AssignTaskRequestValidator : AbstractValidator<AssignTaskRequest>
{
    public AssignTaskRequestValidator()
    {
        RuleFor(x => x.AssignedToUserId)
            .NotEqual(Guid.Empty).WithMessage("Assignee user id must be a valid user or null.")
            .When(x => x.AssignedToUserId.HasValue);
    }
}
